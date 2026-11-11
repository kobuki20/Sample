using UnityEngine;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities;
using System.Collections.Specialized;
using System.Security.Cryptography;

public class LeftHandBackIcon : MonoBehaviour
{
    [Header("表示するアイコン")]
    [SerializeField] private GameObject iconVisual;

    [Header("手の甲からの距離（メートル）")]
    [SerializeField] private float handOffset = 0.035f;

    [Header("表示する角度")]
    [Range(0f, 90f)]
    [SerializeField] private float showAngle = 50f;

    [Header("非表示にする角度")]
    [Range(0f, 90f)]
    [SerializeField] private float hideAngle = 70f;

    [Header("手の甲の法線方向を反転")]
    [SerializeField] private bool reverseNormal = false;

    private Camera mainCamera;
    private bool isShowing = false;

    void Awake()
    {
        mainCamera = Camera.main;
        SetVisible(false);
    }

    void LateUpdate()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            SetVisible(false);
            return;
        }

        // 左手の関節情報を取得
        bool hasPalm = HandJointUtils.TryGetJointPose(
            TrackedHandJoint.Palm,
            Handedness.Left,
            out MixedRealityPose palm);

        bool hasWrist = HandJointUtils.TryGetJointPose(
            TrackedHandJoint.Wrist,
            Handedness.Left,
            out MixedRealityPose wrist);

        bool hasMiddle = HandJointUtils.TryGetJointPose(
            TrackedHandJoint.MiddleKnuckle,
            Handedness.Left,
            out MixedRealityPose middle);

        bool hasIndex = HandJointUtils.TryGetJointPose(
            TrackedHandJoint.IndexKnuckle,
            Handedness.Left,
            out MixedRealityPose index);

        bool hasPinky = HandJointUtils.TryGetJointPose(
            TrackedHandJoint.PinkyKnuckle,
            Handedness.Left,
            out MixedRealityPose pinky);

        // 手が追跡できていなければ非表示
        if (!hasPalm || !hasWrist || !hasMiddle || !hasIndex || !hasPinky)
        {
            SetVisible(false);
            return;
        }

        // 手首から中指の付け根への方向
        Vector3 fingerDirection = middle.Position - wrist.Position;

        // 小指から人差し指への方向
        Vector3 acrossHand = index.Position - pinky.Position;

        if (fingerDirection.sqrMagnitude < 0.000001f || acrossHand.sqrMagnitude < 0.000001f)
        {
            SetVisible(false);
            return;
        }

        fingerDirection.Normalize();
        acrossHand.Normalize();

        // 左手の甲の表面に垂直な方向を求める
        Vector3 backNormal = -(palm.Rotation * Vector3.up);

        if (reverseNormal)
        {
            backNormal = -backNormal;
        }

        backNormal.Normalize();

        // 左手からHololensのカメラへの方向
        Vector3 toCamera = mainCamera.transform.position - palm.Position;

        if (toCamera.sqrMagnitude < 0.000001f)
        {
            SetVisible(false);
            return;
        }

        toCamera.Normalize();

        // 手の甲がカメラに向いている角度
        float angle = Vector3.Angle(backNormal, toCamera);

        // 表示・非表示で異なる角度を使い、境界付近のちらつきを抑える
        float limit = isShowing ? hideAngle : showAngle;
        bool shouldShow = angle <= limit;

        // 手の甲の少し上にアイコンを配置
        transform.position = palm.Position + backNormal * handOffset;

        // アイコンの正面をカメラ方向に向ける
        transform.rotation = Quaternion.LookRotation(toCamera, fingerDirection);

        SetVisible(shouldShow);
    }

    private void SetVisible(bool visible)
    {
        isShowing = visible;

        if (iconVisual != null && iconVisual.activeSelf != visible)
        {
            iconVisual.SetActive(visible);
        }
    }
}
