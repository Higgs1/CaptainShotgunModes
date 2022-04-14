using BepInEx;
using BepInEx.Configuration;
using EntityStates.Captain.Weapon;
using R2API.Utils;
using RiskOfOptions;
using RoR2;
using RoR2.UI;
using System;
using UnityEngine;
using On_ChargeCaptainShotgun = On.EntityStates.Captain.Weapon.ChargeCaptainShotgun;

namespace CaptainShotgunModes {
  enum FireMode { Normal, Auto, AutoCharge, Count }

  [BepInDependency("com.bepis.r2api", BepInDependency.DependencyFlags.HardDependency)]
  [BepInDependency("com.rune580.riskofoptions")]
  [BepInPlugin("de.userstorm.captainshotgunmodes", "CaptainShotgunModes", "1.2.0")]
  [NetworkCompatibility(CompatibilityLevel.NoNeedForSync, VersionStrictness.DifferentModVersionsAreOk)]
  public class CaptainShotgunModesPlugin: BaseUnityPlugin {
    public static ConfigEntry<string> DefaultMode { get; set; }
    public static ConfigEntry<bool> EnableModeSelectionWithNumberKeys { get; set; }
    public static ConfigEntry<bool> EnableModeSelectionWithMouseWheel { get; set; }
    public static ConfigEntry<bool> EnableModeSelectionWithDPad { get; set; }

    private FireMode fireMode = FireMode.Normal;
    private float fixedAge = 0;

    private void SingleFireMode(On_ChargeCaptainShotgun.orig_FixedUpdate orig, ChargeCaptainShotgun self) {
      orig.Invoke(self);

      if (self.GetFieldValue <bool> ("released")) {
        fixedAge = 0;
      }
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

      if (didFire) {
        self.SetFieldValue("released", false);
      }
    }

    private void AutoFireChargeMode(On_ChargeCaptainShotgun.orig_FixedUpdate orig, ChargeCaptainShotgun self) {
      var didFire = false;
      var released = self.GetFieldValue <bool> ("released");
      var chargeDuration = self.GetFieldValue < float > ("chargeDuration");

      if (!released && fixedAge >= chargeDuration) {
        didFire = true;
        fixedAge = 0;
        self.SetFieldValue("released", true);
      }

      orig.Invoke(self);

      if (didFire) {
        self.SetFieldValue("released", false);
      }
    }

    private void CycleFireMode(bool forward = true) {
      FireMode newFireMode = fireMode + (forward ? 1 : -1);

      if (newFireMode == FireMode.Count) {
        newFireMode = FireMode.Normal;
      }

      if ((int) newFireMode == -1) {
        newFireMode = FireMode.Count - 1;
      }

      fireMode = newFireMode;
    }

    private void SelectFireModeWithNumberKeys() {
      if (EnableModeSelectionWithNumberKeys.Value) {
        if (Input.GetKeyDown(KeyCode.Alpha1)) {
          fireMode = FireMode.Normal;
        } else if (Input.GetKeyDown(KeyCode.Alpha2)) {
          fireMode = FireMode.Auto;
        } else if (Input.GetKeyDown(KeyCode.Alpha3)) {
          fireMode = FireMode.AutoCharge;
        }
      }
    }

    private void SelectFireModeWithMouseWheel() {
      if (EnableModeSelectionWithMouseWheel.Value) {
        float wheel = Input.GetAxis("Mouse ScrollWheel");

        if (wheel != 0) {
          // scroll down => forward; scroll up => backward
          CycleFireMode(wheel < 0f);
        }
      }
    }

    private void SelectFireModeWithDPad() {
      if (EnableModeSelectionWithDPad.Value) {
        if (DPad.GetInputDown(DPadInput.Right) || DPad.GetInputDown(DPadInput.Down)) {
          CycleFireMode();
        } else if (DPad.GetInputDown(DPadInput.Left) || DPad.GetInputDown(DPadInput.Up)) {
          CycleFireMode(false);
        }
      }
    }

    public void FixedUpdateHook(On_ChargeCaptainShotgun.orig_FixedUpdate orig, ChargeCaptainShotgun self) {
      fixedAge += Time.fixedDeltaTime;

      switch (fireMode) {
        case FireMode.Auto:
          AutoFireMode(orig, self);
          break;
        case FireMode.AutoCharge:
          AutoFireChargeMode(orig, self);
          break;
        default:
          // fallback to single fire mode
          SingleFireMode(orig, self);
          break;
      }
    }

    public void UpdateHook(On.RoR2.UI.SkillIcon.orig_Update orig, SkillIcon self) {
      orig.Invoke(self);

      if (self.targetSkill && self.targetSkillSlot == SkillSlot.Primary) {
        if (self.targetSkill.characterBody.baseNameToken == "CAPTAIN_BODY_NAME") {
          self.stockText.gameObject.SetActive(true);
          self.stockText.fontSize = 12f;
          self.stockText.SetText(fireMode.ToString());
        }
      }
    }

    public void Awake() {
      DefaultMode = Config.Bind <string> (
        "Settings",
        "DefaultMode",
        "Normal",
        "The mode that is selected on game start. Modes: Normal, Auto, AutoCharge"
      );

      EnableModeSelectionWithNumberKeys = Config.Bind <bool> (
        "Settings",
        "EnableModeSelectionWithNumberKeys",
        true,
        "When set to true modes can be selected using the number keys"
      );

      EnableModeSelectionWithMouseWheel = Config.Bind <bool> (
        "Settings",
        "EnableModeSelectionWithMouseWheel",
        true,
        "When set to true modes can be cycled through using the mouse wheel"
      );

      EnableModeSelectionWithDPad = Config.Bind <bool> (
        "Settings",
        "EnableModeSelectionWithDPad",
        true,
        "When set to true modes can be cycled through using the DPad (controller)"
      );
      
      try {
        fireMode = (FireMode) Enum.Parse(typeof (FireMode), DefaultMode.Value);
      } catch (Exception) {
        fireMode = FireMode.Normal;
      }

      On_ChargeCaptainShotgun.FixedUpdate += FixedUpdateHook;
      On.RoR2.UI.SkillIcon.Update += UpdateHook;
    }

    public void Update() {
      DPad.Update();

      SelectFireModeWithNumberKeys();
      SelectFireModeWithMouseWheel();
      SelectFireModeWithDPad();
    }

    public void OnDestroy() {
      On_ChargeCaptainShotgun.FixedUpdate -= FixedUpdateHook;
      On.RoR2.UI.SkillIcon.Update -= UpdateHook;
    }
  }
}
