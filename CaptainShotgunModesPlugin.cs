using BepInEx;
using BepInEx.Configuration;
using EntityStates.Captain.Weapon;
using R2API.Utils;
using RoR2;
using RoR2.UI;
using UnityEngine;
using On_ChargeCaptainShotgun = On.EntityStates.Captain.Weapon.ChargeCaptainShotgun;

namespace CaptainShotgunModes {
  public enum FireMode { Normal, Auto, AutoCharge }

  [BepInDependency("com.bepis.r2api", BepInDependency.DependencyFlags.HardDependency)]
  [BepInDependency("com.rune580.riskofoptions", BepInDependency.DependencyFlags.SoftDependency)]
  [BepInPlugin("CaptainShotgunModes", "Captain Shotgun Modes", "1.2.1")]
  [NetworkCompatibility(CompatibilityLevel.NoNeedForSync, VersionStrictness.DifferentModVersionsAreOk)]
  public class CaptainShotgunModesPlugin: BaseUnityPlugin {
    public static ConfigEntry<FireMode> DefaultMode;
    public static ConfigEntry<bool> EnableModeSelectionWithMouseWheel;
    public static ConfigEntry<bool> EnableModeSelectionWithDPad;
    public static ConfigEntry<KeyboardShortcut> SelectNormalMode;
    public static ConfigEntry<KeyboardShortcut> SelectAutoMode;
    public static ConfigEntry<KeyboardShortcut> SelectAutoChargeMode;

    private static readonly int FireModeCount = System.Enum.GetNames(typeof(FireMode)).Length;

    private FireMode fireMode = FireMode.Normal;
    private float fixedAge = 0;

    private void SingleFireMode(On_ChargeCaptainShotgun.orig_FixedUpdate orig, ChargeCaptainShotgun self) {
      orig.Invoke(self);

      if (self.GetFieldValue <bool> ("released"))
        fixedAge = 0;
    }

    private void AutoFireMode(On_ChargeCaptainShotgun.orig_FixedUpdate orig, ChargeCaptainShotgun self) {
      var didFire = false;
      var released = self.GetFieldValue <bool> ("released");

      if (!released) {
        didFire = true;
        fixedAge = 0;
        self.SetFieldValue("released", true);
      }

      orig.Invoke(self);

      if (didFire)
        self.SetFieldValue("released", false);
    }

    private void AutoFireChargeMode(On_ChargeCaptainShotgun.orig_FixedUpdate orig, ChargeCaptainShotgun self) {
      var didFire = false;
      var released = self.GetFieldValue <bool> ("released");
      var chargeDuration = self.GetFieldValue <float> ("chargeDuration");

      if (!released && fixedAge >= chargeDuration) {
        didFire = true;
        fixedAge = 0;
        self.SetFieldValue("released", true);
      }

      orig.Invoke(self);

      if (didFire)
        self.SetFieldValue("released", false);
    }

    private void CycleFireMode(bool forward = true) {
      FireMode newFireMode = fireMode + (forward ? 1 : -1);

      if ((int) newFireMode == FireModeCount)
        fireMode = FireMode.Normal;
      else if ((int) newFireMode == -1)
        fireMode = FireMode.AutoCharge;
      else
        fireMode = newFireMode;
    }

    public void FixedUpdateHook(On_ChargeCaptainShotgun.orig_FixedUpdate orig, ChargeCaptainShotgun self) {
      fixedAge += Time.fixedDeltaTime;

      switch (fireMode) {
        case FireMode.Auto:
          AutoFireMode(orig, self);
          return;
        case FireMode.AutoCharge:
          AutoFireChargeMode(orig, self);
          return;
        default:
          // fallback to single fire mode
          SingleFireMode(orig, self);
          return;
      }
    }

    public void UpdateHook(On.RoR2.UI.SkillIcon.orig_Update orig, SkillIcon self) {
      orig.Invoke(self);

      if (self.targetSkill && self.targetSkillSlot == SkillSlot.Primary && self.targetSkill.characterBody.baseNameToken == "CAPTAIN_BODY_NAME") {
        self.stockText.gameObject.SetActive(true);
        self.stockText.fontSize = 12f;
        self.stockText.SetText(fireMode.ToString());
      }
    }

    public void Awake() {
      DefaultMode = Config.Bind <FireMode> (
        "",
        "Default Mode",
        FireMode.Normal,
        "The mode that is selected on game start."
      );

      EnableModeSelectionWithMouseWheel = Config.Bind <bool> (
        "",
        "Enable Mode Selection With Mouse Wheel",
        true,
        "When set to true firing modes can be cycled through using the mouse wheel"
      );

      EnableModeSelectionWithDPad = Config.Bind <bool> (
        "",
        "Enable Mode Selection With DPad",
        true,
        "When set to true firing modes can be cycled through using the DPad (controller)"
      );
      
      SelectNormalMode = Config.Bind <KeyboardShortcut> (
        "",
        "Select Normal Mode",
        new KeyboardShortcut(KeyCode.Alpha1),
        "Sets firing mode to Normal."
      );
      
      SelectAutoMode = Config.Bind <KeyboardShortcut> (
        "",
        "Select Auto Mode",
        new KeyboardShortcut(KeyCode.Alpha2),
        "Sets firing mode to Auto."
      );
      
      SelectAutoChargeMode = Config.Bind <KeyboardShortcut> (
        "",
        "Select Auto Charge Mode",
        new KeyboardShortcut(KeyCode.Alpha3),
        "Sets firing mode to Auto Charge."
      );
      
      if (BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.rune580.riskofoptions"))
        InitRoo();

      fireMode = DefaultMode.Value;
      
      On_ChargeCaptainShotgun.FixedUpdate += FixedUpdateHook;
      On.RoR2.UI.SkillIcon.Update += UpdateHook;
    }
    
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    private void InitRoo() {
      RiskOfOptions.ModSettingsManager.SetModDescription(
          "This mod allows you to choose between 3 firing modes for the captain's shotgun"
      );
      RiskOfOptions.ModSettingsManager.AddOption(new RiskOfOptions.Options.ChoiceOption(DefaultMode));
      RiskOfOptions.ModSettingsManager.AddOption(new RiskOfOptions.Options.KeyBindOption(SelectNormalMode));
      RiskOfOptions.ModSettingsManager.AddOption(new RiskOfOptions.Options.KeyBindOption(SelectAutoMode));
      RiskOfOptions.ModSettingsManager.AddOption(new RiskOfOptions.Options.KeyBindOption(SelectAutoChargeMode));
      RiskOfOptions.ModSettingsManager.AddOption(new RiskOfOptions.Options.CheckBoxOption(EnableModeSelectionWithMouseWheel));
      RiskOfOptions.ModSettingsManager.AddOption(new RiskOfOptions.Options.CheckBoxOption(EnableModeSelectionWithDPad));
      
      using (System.IO.Stream stream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("icon")) {
        Texture2D texture = new Texture2D(0, 0);
        byte[] imgdata = new byte[stream.Length];
        stream.Read(imgdata, 0, (int) stream.Length);
        if (ImageConversion.LoadImage(texture, imgdata))
          RiskOfOptions.ModSettingsManager.SetModIcon(
            Sprite.Create(
              texture,
              new Rect(0, 0, texture.width, texture.height),
              new Vector2(0, 0)
            )
          );
      }
    }
    
    private bool IsKeyDown(KeyboardShortcut shortcut) {
      if (shortcut.MainKey == KeyCode.None)
        return false;
      foreach (var i in shortcut.Modifiers)
        if (!Input.GetKey(i))
          return false;
      return Input.GetKeyDown(shortcut.MainKey);
    }

    public void Update() {
      if (IsKeyDown(SelectNormalMode.Value)) 
        fireMode = FireMode.Normal;
      else if (IsKeyDown(SelectAutoMode.Value))
        fireMode = FireMode.Auto;
      else if (IsKeyDown(SelectAutoChargeMode.Value))
        fireMode = FireMode.AutoCharge;
      
      if (EnableModeSelectionWithMouseWheel.Value) {
        float wheel = Input.GetAxis("Mouse ScrollWheel");
        if (wheel != 0)
          CycleFireMode(wheel < 0f);
      }
      
      if (EnableModeSelectionWithDPad.Value) {
        DPad.Update();
        if (DPad.GetInputDown(DPadInput.Right) || DPad.GetInputDown(DPadInput.Down))
          CycleFireMode();
        else if (DPad.GetInputDown(DPadInput.Left) || DPad.GetInputDown(DPadInput.Up))
          CycleFireMode(false);
      }
    }

    public void OnDestroy() {
      On_ChargeCaptainShotgun.FixedUpdate -= FixedUpdateHook;
      On.RoR2.UI.SkillIcon.Update -= UpdateHook;
    }
  }
}
