using System.Text;
using UnityEngine;
using Cinemachine;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// CameraDebug - detailed runtime dump of Cameras, CinemachineBrains, and Virtual Cameras.
/// Attach to an active GameObject in your scene (e.g. CameraManager or an empty named CameraDebug).
/// Press the DumpKey (default: P) to write a full snapshot to the Console immediately.
/// Optionally prints a periodic snapshot controlled by LogInterval or every frame if LogEveryFrame = true.
/// </summary>
public class CameraDebug : MonoBehaviour
{
    [Header("Logging Options")]
    [Tooltip("Seconds between automatic logs. Set to 0 to disable automatic logging.")]
    public float logInterval = 1.0f;

    [Tooltip("If true, print every Update() (very spammy).")]
    public bool logEveryFrame = false;

    [Tooltip("Key to press to produce an immediate debug snapshot.")]
    public KeyCode dumpKey = KeyCode.P;

    [Tooltip("If true, use FindObjectsOfType(includeInactive:true) to also list inactive vcams.")]
    public bool includeInactiveVcams = true;

    float _timer = 0f;

    void Update()
    {
        if (logEveryFrame)
            LogFullSnapshot();

        if (logInterval > 0f)
        {
            _timer += Time.deltaTime;
            if (_timer >= logInterval)
            {
                _timer = 0f;
                LogFullSnapshot();
            }
        }

        if (Input.GetKeyDown(dumpKey))
        {
            LogFullSnapshot();
        }
    }

    void LogFullSnapshot()
    {
        var sb = new StringBuilder();
        sb.AppendLine("===== CAMERA DEBUG SNAPSHOT =====");
        sb.AppendLine($"Time: {Time.time:F2}s  Scene: {UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}");
        sb.AppendLine("");

        // Main Camera + Brain
        Camera mainCam = Camera.main;
        sb.AppendLine("--- Main Camera ---");
        if (mainCam == null)
        {
            sb.AppendLine("Main Camera: NONE (Camera.main is null).");
        }
        else
        {
            sb.AppendLine($"Main Camera GameObject: {mainCam.gameObject.name}");
            sb.AppendLine($"  tag: {mainCam.tag}  activeInHierarchy: {mainCam.gameObject.activeInHierarchy}  enabled: {mainCam.enabled}");
            sb.AppendLine($"  position: {mainCam.transform.position} rotation: {mainCam.transform.eulerAngles}");
            var brain = mainCam.GetComponent<CinemachineBrain>();
            if (brain == null)
            {
                sb.AppendLine("  CinemachineBrain: NONE on Main Camera.");
            }
            else
            {
                sb.AppendLine($"  CinemachineBrain: present  enabled: {brain.enabled}");
                // Active Virtual Camera (may be null)
                var activeVCam = brain.ActiveVirtualCamera;
                if (activeVCam == null)
                {
                    sb.AppendLine("  Brain.ActiveVirtualCamera: NULL (no live vcam)");
                }
                else
                {
                    var liveGO = activeVCam.VirtualCameraGameObject != null ? activeVCam.VirtualCameraGameObject.name : "NULL";
                    sb.AppendLine($"  Brain.ActiveVirtualCamera: {liveGO}");
                }
            }
        }

        sb.AppendLine("");

        // All Cameras in scene (real Camera components)
        sb.AppendLine("--- All Camera Components (Camera.allCameras) ---");
        Camera[] cams = Camera.allCameras;
        if (cams == null || cams.Length == 0)
        {
            sb.AppendLine("No Camera components found via Camera.allCameras.");
        }
        else
        {
            for (int i = 0; i < cams.Length; i++)
            {
                var c = cams[i];
                sb.AppendLine($"[{i}] Name: {c.gameObject.name}  activeInHierarchy: {c.gameObject.activeInHierarchy}  enabled: {c.enabled}");
                sb.AppendLine($"    tag: {c.tag}  cullingMask: {c.cullingMask}  depth: {c.depth}");
                var b = c.GetComponent<CinemachineBrain>();
                sb.AppendLine($"    Has CinemachineBrain component? {(b != null ? "YES" : "NO")}  (enabled: {(b != null ? b.enabled.ToString() : "N/A")})");
                sb.AppendLine($"    Transform: pos {c.transform.position} rot {c.transform.eulerAngles}");
            }
        }

        sb.AppendLine("");

        // All CinemachineBrains in scene (find all)
        sb.AppendLine("--- CinemachineBrains (all) ---");
        var brains = FindObjectsOfType<CinemachineBrain>(true);
        if (brains == null || brains.Length == 0)
        {
            sb.AppendLine("No CinemachineBrain components found in the scene.");
        }
        else
        {
            for (int i = 0; i < brains.Length; i++)
            {
                var br = brains[i];
                sb.AppendLine($"[{i}] Brain on GameObject: {br.gameObject.name}  activeInHierarchy: {br.gameObject.activeInHierarchy} enabled: {br.enabled}");
                var av = br.ActiveVirtualCamera;
                sb.AppendLine($"     ActiveVirtualCamera: {(av == null ? "NULL" : av.VirtualCameraGameObject.name)}");
            }
        }

        sb.AppendLine("");

        // All CinemachineVirtualCameras
        sb.AppendLine("--- CinemachineVirtualCameras (all, includeInactive=" + includeInactiveVcams + ") ---");
        CinemachineVirtualCamera[] allVcams;
        if (includeInactiveVcams)
            allVcams = FindObjectsOfType<CinemachineVirtualCamera>(true);
        else
            allVcams = FindObjectsOfType<CinemachineVirtualCamera>();

        if (allVcams == null || allVcams.Length == 0)
        {
            sb.AppendLine("No CinemachineVirtualCamera instances found.");
        }
        else
        {
            for (int i = 0; i < allVcams.Length; i++)
            {
                var v = allVcams[i];
                sb.AppendLine($"[{i}] VCam name: {v.gameObject.name}");
                sb.AppendLine($"    activeInHierarchy: {v.gameObject.activeInHierarchy}  activeSelf: {v.gameObject.activeSelf}  enabled: {v.enabled}");
                sb.AppendLine($"    Priority: {v.Priority}  Transform pos: {v.transform.position} rot: {v.transform.eulerAngles}");
                sb.AppendLine($"    Parent: {(v.transform.parent != null ? v.transform.parent.name : "NONE")}");
                var follow = v.Follow != null ? v.Follow.name : "NULL";
                var lookAt = v.LookAt != null ? v.LookAt.name : "NULL";
                sb.AppendLine($"    Follow: {follow}   LookAt: {lookAt}");
                var camComp = v.GetComponent<Camera>();
                sb.AppendLine($"    Has Camera component? {(camComp != null ? "YES" : "NO")}");
                var rogueBrain = v.GetComponent<CinemachineBrain>();
                sb.AppendLine($"    Has CinemachineBrain on same GameObject? {(rogueBrain != null ? "YES" : "NO")}");
#if UNITY_EDITOR
                // Try to show which VirtualCamera is set as "Live" in editor (if possible)
                // (we already show brain.ActiveVirtualCamera above)
#endif
            }
        }

        sb.AppendLine("");
        sb.AppendLine("===== END CAMERA DEBUG SNAPSHOT =====");

        Debug.Log(sb.ToString());
    }
}
