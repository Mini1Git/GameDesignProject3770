using System;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

public static class CombatVerification
{
    private const BindingFlags Fields = BindingFlags.Instance | BindingFlags.NonPublic;
    private static int passed;

    [MenuItem("Tools/Combat/Verify Combat Contracts")]
    public static void Run()
    {
        passed = 0;
        try
        {
            Check("light works at zero stamina and rewards only one valid hit", f =>
            {
                f.stamina.Stamina = 0f;
                f.Press();
                Equal(f.stamina.Stamina, 0f);
                Expect(!Get<bool>(f.stamina, "combatBlocksRegen"), "pending press blocked recovery");
                f.combat.OnAttackReleased();
                Expect(f.Last.Kind == MeleeAttackKind.Light, "expected light");
                Equal(f.stamina.Stamina, 0f);
                f.combat.NotifyValidHit(f.Last.AttackId + 10);
                Equal(f.stamina.Stamina, 0f);
                f.combat.NotifyValidHit(f.Last.AttackId);
                f.combat.NotifyValidHit(f.Last.AttackId);
                Equal(f.stamina.Stamina, 2f);
                Expect(Get<bool>(f.stamina, "combatBlocksRegen"), "hit unlocked natural recovery");
            });

            Check("charging drains by time and release adds no cost", f =>
            {
                f.Press();
                f.Advance(0.6);
                Equal(f.stamina.Stamina, 7.6f);
                Equal(f.combat.ChargeRatio, 0.5f);
                f.combat.OnAttackReleased();
                Expect(f.Last.Kind == MeleeAttackKind.Heavy, "expected heavy");
                Equal(f.Last.DamageMultiplier, 1.5f);
                Equal(f.Last.Damage, 6f);
                Equal(f.stamina.Stamina, 7.6f);
                f.combat.NotifyValidHit(f.Last.AttackId);
                Equal(f.stamina.Stamina, 7.6f);
            });

            Check("release processes elapsed charge before the next update", f =>
            {
                f.Press();
                double start = Time.timeAsDouble - 0.6;
                Set(f.combat, "chargeStartsAt", start);
                Set(f.combat, "lastChargeUpdate", start);
                f.combat.OnAttackReleased();
                Expect(f.Last.Kind == MeleeAttackKind.Heavy, "release became a light attack");
                Equal(f.stamina.Stamina, 7.6f);
            });

            Check("full charge fires once and waits for a new press", f =>
            {
                f.Press();
                f.Advance(5);
                Equal(f.stamina.Stamina, 5.2f);
                Equal(f.Last.ChargeRatio, 1f);
                Equal(f.Last.Damage, 8f);
                f.combat.NotifyAttackFinished(f.Last.AttackId);
                f.combat.OnAttackPressed();
                f.Advance(10);
                f.combat.OnAttackReleased();
                Expect(f.executor.requests.Count == 1, "held input or release repeated the attack");
                f.Press();
                f.combat.OnAttackReleased();
                Expect(f.executor.requests.Count == 2, "new press did not rearm");
            });

            Check("low stamina only buys an affordable partial charge", f =>
            {
                f.stamina.Stamina = 1f;
                f.Press();
                f.Advance(2);
                Equal(f.stamina.Stamina, 0f);
                Equal(f.Last.ChargeRatio, 0.25f / 1.2f);
                Expect(f.Last.Kind == MeleeAttackKind.Heavy, "exhaustion did not release heavy");
                f.combat.OnAttackReleased();
                Expect(f.executor.requests.Count == 1, "exhaustion fired twice");
            });

            Check("zero stamina cannot enter a heavy charge", f =>
            {
                f.stamina.Stamina = 0f;
                f.Press();
                f.Advance(1);
                f.combat.OnAttackReleased();
                Expect(!f.combat.IsBusy && f.executor.requests.Count == 0, "free heavy at zero stamina");
            });

            Check("weapon bonuses and weapon switches use a press time snapshot", f =>
            {
                f.weapon.extraFullChargeBonus = 0.25f;
                f.Press();
                f.data.heavyBaseDamage = 100;
                f.data.chargeStaminaPerSecond = 100;
                f.data.maxChargeDamageMultiplier = 10;
                f.weapon.extraFullChargeBonus = 5;
                f.inventory.quickSlots[0] = null;
                f.Advance(2);
                Equal(f.Last.DamageMultiplier, 2.5f);
                Equal(f.Last.Damage, 10f);
                Equal(f.stamina.Stamina, 5.2f);
            });

            Check("recovery stays blocked until completion then waits 1.5 seconds", f =>
            {
                f.stamina.Stamina = 1;
                f.Press();
                f.combat.OnAttackReleased();
                f.Regen(Time.timeAsDouble + 100, 1);
                Equal(f.stamina.Stamina, 1);
                f.combat.NotifyAttackFinished(f.Last.AttackId);
                double allowed = Get<double>(f.stamina, "regenAllowedAt");
                Equal((float)(allowed - Time.timeAsDouble), 1.5f);
                f.Regen(allowed - 0.01, 1);
                Equal(f.stamina.Stamina, 1);
                f.Regen(allowed + 0.1, 0.2f);
                Equal(f.stamina.Stamina, 1.35f);
                f.stamina.EndCombatAction();
                Expect(Get<double>(f.stamina, "regenAllowedAt") == allowed, "duplicate finish reset delay");
            });

            Check("another attack blocks an existing recovery delay", f =>
            {
                f.stamina.Stamina = 1;
                f.Press();
                f.combat.OnAttackReleased();
                f.combat.NotifyAttackFinished(f.Last.AttackId);
                double oldDeadline = Get<double>(f.stamina, "regenAllowedAt");
                f.Press();
                f.combat.OnAttackReleased();
                f.Regen(oldDeadline + 10, 1);
                Equal(f.stamina.Stamina, 1);
                f.combat.NotifyValidHit(f.Last.AttackId);
                Equal(f.stamina.Stamina, 3);
            });

            Check("cancellation releases recovery and ignores stale feedback", f =>
            {
                f.stamina.Stamina = 1;
                f.Press();
                f.combat.OnAttackReleased();
                int oldId = f.Last.AttackId;
                f.combat.CancelCombat();
                Expect(f.executor.cancelCount == 1, "executor was not canceled");
                f.combat.NotifyValidHit(oldId);
                Equal(f.stamina.Stamina, 1);
                f.Press();
                f.combat.OnAttackReleased();
                f.combat.NotifyAttackFinished(oldId);
                Expect(f.combat.IsBusy, "stale completion ended a new attack");
            });

            Check("blocked controls cancel charging before release", f =>
            {
                f.Press();
                f.Advance(0.3);
                f.player.canAct = false;
                f.combat.OnAttackReleased();
                Expect(!f.combat.IsBusy && f.executor.requests.Count == 0, "blocked release attacked");
                Expect(!Get<bool>(f.stamina, "combatBlocksRegen"), "recovery stayed locked");
            });

            Check("focus loss and disabling cancel a charge", f =>
            {
                f.Press();
                f.Advance(0.3);
                Call(f.combat, "OnApplicationFocus", false);
                f.combat.OnAttackReleased();
                Expect(!f.combat.IsBusy && f.executor.requests.Count == 0, "focus loss attacked");
                f.Press();
                f.Advance(0.3);
                Call(f.combat, "OnDisable");
                Expect(!Get<bool>(f.stamina, "combatBlocksRegen"), "disable left recovery locked");
            });

            Check("missing or rejecting executors cannot leave combat busy", f =>
            {
                Set(f.combat, "attackExecutor", null);
                f.Press();
                f.combat.OnAttackReleased();
                Expect(!f.combat.IsBusy, "missing executor started combat");
                Equal(f.stamina.Stamina, 10);
                Set(f.combat, "attackExecutor", f.executor);
                f.executor.accept = false;
                f.Press();
                f.combat.OnAttackReleased();
                Expect(!f.combat.IsBusy, "rejected request stayed busy");
                Expect(!Get<bool>(f.stamina, "combatBlocksRegen"), "rejected request locked recovery");
            });

            Check("synchronous completion is safe", f =>
            {
                f.executor.completeImmediately = true;
                f.Press();
                f.Advance(2);
                f.combat.OnAttackReleased();
                Expect(!f.combat.IsBusy && f.executor.requests.Count == 1, "immediate completion repeated attack");
            });

            Check("right click still spends mana once after input reenable", f =>
            {
                var mana = f.root.AddComponent<Mana_System>();
                Set(mana, "player", f.player);
                mana.Mana = 10;
                f.player.mana_System = mana;
                Mouse mouse = InputSystem.AddDevice<Mouse>();
                // input actions normally ignore editor updates; restore this test override afterward
                object manager = typeof(InputSystem).GetField("s_Manager", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);
                PropertyInfo editModeUpdates = manager.GetType().GetProperty("runPlayerUpdatesInEditMode");
                object previousUpdates = editModeUpdates.GetValue(manager);
                try
                {
                    editModeUpdates.SetValue(manager, true);
                    for (int i = 0; i < 3; i++)
                    {
                        Call(f.player, "OnEnable");
                        Call(f.player, "OnDisable");
                    }
                    Call(f.player, "OnEnable");
                    InputSystem.QueueStateEvent(mouse, new MouseState().WithButton(MouseButton.Right));
                    InputSystem.Update();
                    Equal(mana.Mana, 5);
                    Equal(f.stamina.Stamina, 10);
                }
                finally
                {
                    Call(f.player, "OnDisable");
                    InputSystem.RemoveDevice(mouse);
                    editModeUpdates.SetValue(manager, previousUpdates);
                }
            });

            VerifySwordPattern();
            VerifyAssets();
            Debug.Log($"Combat verification passed: {passed} checks.");
            if (Application.isBatchMode) EditorApplication.Exit(0);
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
            if (Application.isBatchMode) EditorApplication.Exit(1);
            else throw;
        }
    }

    private static void VerifyAssets()
    {
        var swordDefaults = AssetDatabase.LoadAssetAtPath<WeaponCombatData>("Assets/Combat/Weapons/SwordDefaults.asset");
        Expect(swordDefaults != null, "missing sword defaults");

        foreach (string path in new[] { "Assets/Items/LongSword.asset", "Assets/Items/KatanaSword.asset" })
        {
            var item = AssetDatabase.LoadAssetAtPath<itemData>(path);
            Expect(item != null && item.weaponDefinition == null && item.weaponDefaults == swordDefaults,
                $"an existing sword is not linked directly to its class defaults: {path}");
        }

        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Player.prefab");
        var combat = prefab.GetComponent<PlayerCombat>();
        Expect(combat != null && Get<player>(combat, "playerController") == prefab.GetComponent<player>()
            && Get<inventory>(combat, "playerInventory") == prefab.GetComponent<inventory>(), "invalid combat prefab links");
        var executor = prefab.GetComponent<SwordAttackExecutor>();
        Expect(executor != null && Get<MonoBehaviour>(combat, "attackExecutor") == executor,
            "missing sword executor binding");
        Expect(swordDefaults.lightPattern != null && swordDefaults.heavyPattern != null
            && swordDefaults.lightPattern != swordDefaults.heavyPattern
            && swordDefaults.lightPattern.slashSprite != null && swordDefaults.heavyPattern.slashSprite != null,
            "missing sword pattern or sprite reference");
        Expect(AssetDatabase.GetAssetPath(swordDefaults.lightPattern.slashSprite)
            == "Assets/Art/Effects/SwordSlashLight.png"
            && AssetDatabase.GetAssetPath(swordDefaults.heavyPattern.slashSprite)
            == "Assets/Art/Effects/SwordSlashHeavy.png", "incorrect light or heavy sprite binding");

        Scene scene = EditorSceneManager.OpenPreviewScene("Assets/Scenes/testingArea.unity");
        try
        {
            player scenePlayer = null;
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                player candidate = root.GetComponentInChildren<player>(true);
                if (candidate != null) scenePlayer = candidate;
            }
            Expect(scenePlayer != null && scenePlayer.stamina_System != null
                && scenePlayer.mana_System != null && scenePlayer.GetComponent<PlayerCombat>() != null,
                "testing scene has missing resource or combat links");
        }
        finally { EditorSceneManager.ClosePreviewScene(scene); }
        passed++;
        Debug.Log("PASS: sword defaults, patterns, sprites, prefab and scene references");
    }

    private static void VerifySwordPattern()
    {
        Check("sword patterns display, damage, restore stamina and finish", f =>
        {
            f.root.transform.position = new Vector3(1000f, 1000f, 0f);
            var defaults = AssetDatabase.LoadAssetAtPath<WeaponCombatData>("Assets/Combat/Weapons/SwordDefaults.asset");
            var executor = f.root.AddComponent<SwordAttackExecutor>();
            Set(executor, "slashMaterial", AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/Effects/SwordSlash.mat"));
            Set(f.combat, "attackExecutor", executor);
            itemData equipped = f.inventory.quickSlots[0];
            equipped.weaponDefinition = null;
            equipped.weaponDefaults = defaults;
            f.stamina.Stamina = 5;

            GameObject body = new GameObject("sword hit target");
            GameObject trigger = new GameObject("detection only target");
            GameObject behind = new GameObject("target behind player");
            try
            {
                body.transform.position = f.root.transform.position + Vector3.right * 1.1f;
                body.AddComponent<BoxCollider2D>().size = Vector2.one * 0.3f;
                body.AddComponent<CircleCollider2D>().radius = 0.15f;
                enemyBase enemy = body.AddComponent<enemyBase>();
                enemy.health = 50;
                trigger.transform.position = f.root.transform.position + Vector3.right * 5f;
                var detector = trigger.AddComponent<CircleCollider2D>();
                detector.radius = 8;
                detector.isTrigger = true;
                enemyBase triggerEnemy = trigger.AddComponent<enemyBase>();
                behind.transform.position = f.root.transform.position - Vector3.right;
                behind.AddComponent<BoxCollider2D>();
                enemyBase behindEnemy = behind.AddComponent<enemyBase>();
                Physics2D.SyncTransforms();

                f.Press();
                f.combat.OnAttackReleased();
                Call(executor, "AdvancePattern", defaults.lightPattern.windup + defaults.lightPattern.activeDuration * 0.5f);
                Equal(enemy.health, 48);
                Equal(triggerEnemy.health, 10);
                Equal(behindEnemy.health, 10);
                Equal(f.stamina.Stamina, 7);
                GameObject visual = Get<GameObject>(executor, "visual");
                Expect(visual.GetComponent<SpriteRenderer>().enabled || visual.GetComponent<LineRenderer>().enabled,
                    "slash has no visible renderer");
                CaptureSlash(visual, "sword-light");
                Call(executor, "AdvancePattern", 2f);
                Equal(enemy.health, 48);
                Expect(!f.combat.IsBusy, "pattern did not release combat after recovery");

                f.Press();
                f.Advance(0.6);
                f.combat.OnAttackReleased();
                float heavyTime = defaults.heavyPattern.windup + defaults.heavyPattern.activeDuration * 0.5f;
                Call(executor, "AdvancePattern", heavyTime);
                Equal(enemy.health, 42);
                Equal(f.stamina.Stamina, 4.6f);
                Vector3 partialSize = visual.transform.localScale;
                Vector3 partialPosition = visual.transform.position;
                CaptureSlash(visual, "sword-heavy");
                f.combat.CancelCombat();
                Call(executor, "AdvancePattern", 5f);
                Equal(enemy.health, 42);
                Expect(!visual.GetComponent<SpriteRenderer>().enabled && !visual.GetComponent<LineRenderer>().enabled,
                    "canceled slash is still visible");

                f.stamina.Stamina = 10;
                f.Press();
                f.Advance(2);
                Call(executor, "AdvancePattern", heavyTime);
                Equal(enemy.health, 34);
                Equal(f.stamina.Stamina, 5.2f);
                Expect(visual.transform.localScale == partialSize && visual.transform.position == partialPosition,
                    "charge level changed the heavy pattern instead of only damage");
                Call(executor, "AdvancePattern", 2f);
                f.combat.OnAttackReleased();
                Expect(!f.combat.IsBusy, "full heavy did not finish");
            }
            finally
            {
                Object.DestroyImmediate(body);
                Object.DestroyImmediate(trigger);
                Object.DestroyImmediate(behind);
            }
        });
    }

    private static void CaptureSlash(GameObject visual, string name)
    {
        bool requested = Array.Exists(Environment.GetCommandLineArgs(),
            arg => string.Equals(arg, "-captureSwordSlash", StringComparison.OrdinalIgnoreCase));
        if ((!requested && !Application.isBatchMode) || SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null) return;
        var cameraObject = new GameObject("slash capture camera");
        var target = new RenderTexture(512, 512, 24);
        var image = new Texture2D(512, 512, TextureFormat.RGBA32, false);
        RenderTexture previous = RenderTexture.active;
        try
        {
            var camera = cameraObject.AddComponent<Camera>();
            camera.scene = visual.scene;
            camera.transform.position = visual.transform.position + new Vector3(0f, 0f, -10f);
            camera.orthographic = true;
            camera.orthographicSize = 2f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.025f, 0.035f, 0.055f);
            camera.enabled = false;
            target.Create();
            if (GraphicsSettings.currentRenderPipeline != null)
                RenderPipeline.SubmitRenderRequest(camera, new RenderPipeline.StandardRequest { destination = target });
            else
            {
                camera.targetTexture = target;
                camera.Render();
            }
            RenderTexture.active = target;
            image.ReadPixels(new Rect(0, 0, 512, 512), 0, 0);
            image.Apply();
            string directory = System.IO.Path.GetFullPath(System.IO.Path.Combine(Application.dataPath, "../Temp/CombatVerification"));
            System.IO.Directory.CreateDirectory(directory);
            string path = System.IO.Path.Combine(directory, name + ".png");
            System.IO.File.WriteAllBytes(path, image.EncodeToPNG());
            Debug.Log("Slash capture: " + path);
            int visiblePixels = 0;
            foreach (Color pixel in image.GetPixels())
                if (pixel.r > 0.5f || pixel.g > 0.5f || pixel.b > 0.5f) visiblePixels++;
            Expect(visiblePixels > 20, "slash did not render into the capture");
        }
        finally
        {
            RenderTexture.active = previous;
            Object.DestroyImmediate(cameraObject);
            Object.DestroyImmediate(image);
            target.Release();
            Object.DestroyImmediate(target);
        }
    }

    private static void Check(string name, Action<Fixture> test)
    {
        using (var fixture = new Fixture()) test(fixture);
        passed++;
        Debug.Log("PASS: " + name);
    }

    private static void Expect(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }

    private static void Equal(float actual, float expected)
    {
        Expect(Mathf.Abs(actual - expected) < 0.001f, $"expected {expected}, received {actual}");
    }

    private static T Get<T>(object target, string field) => (T)target.GetType().GetField(field, Fields).GetValue(target);
    private static void Set(object target, string field, object value) => target.GetType().GetField(field, Fields).SetValue(target, value);
    private static void Call(object target, string method, params object[] args) =>
        target.GetType().GetMethod(method, Fields).Invoke(target, args);

    private sealed class Fixture : IDisposable
    {
        public readonly GameObject root = new GameObject("combat verification");
        public readonly player player;
        public readonly inventory inventory;
        public readonly Stamina_System stamina;
        public readonly PlayerCombat combat;
        public readonly CombatTestExecutor executor;
        public readonly WeaponCombatData data = ScriptableObject.CreateInstance<WeaponCombatData>();
        public readonly WeaponDefinition weapon = ScriptableObject.CreateInstance<WeaponDefinition>();
        private readonly itemData item = ScriptableObject.CreateInstance<itemData>();
        private readonly Scene scene;

        public MeleeAttackRequest Last => executor.requests[executor.requests.Count - 1];

        public Fixture()
        {
            scene = EditorSceneManager.NewPreviewScene();
            SceneManager.MoveGameObjectToScene(root, scene);
            root.AddComponent<Rigidbody2D>();
            player = root.AddComponent<player>();
            inventory = root.AddComponent<inventory>();
            stamina = root.AddComponent<Stamina_System>();
            combat = root.AddComponent<PlayerCombat>();
            executor = root.AddComponent<CombatTestExecutor>();
            player.stamina_System = stamina;
            Set(stamina, "player", player);
            stamina.Stamina = 10;
            Set(combat, "attackExecutor", executor);
            Call(combat, "Awake");
            Call(player, "Awake");
            weapon.combatDefaults = data;
            item.type = itemType.weapon;
            item.weaponDefinition = weapon;
            inventory.quickSlots[0] = item;
        }

        public void Press() => combat.OnAttackPressed();
        public void Advance(double chargeSeconds) => Call(combat, "AdvanceCharge",
            Get<double>(combat, "chargeStartsAt") + chargeSeconds);
        public void Regen(double now, float deltaTime) => Call(stamina, "RegenerateStaminaForFrame", now, deltaTime);

        public void Dispose()
        {
            combat.CancelCombat();
            Call(player, "OnDisable");
            Object.DestroyImmediate(root);
            Object.DestroyImmediate(item);
            Object.DestroyImmediate(weapon);
            Object.DestroyImmediate(data);
            EditorSceneManager.ClosePreviewScene(scene);
        }
    }
}
