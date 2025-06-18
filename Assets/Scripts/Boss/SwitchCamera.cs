using UnityEngine;
using Cinemachine;
using System.Collections;

public class SwitchCamera : MonoBehaviour
{
    public CinemachineVirtualCamera virtualCamera; // Kéo Virtual Camera vào Inspector
    public Transform target1; // Mục tiêu 1
    public Transform target2; // Mục tiêu 2
    private bool isFollowingTarget1 = true;
    [SerializeField] private float timeSwitch = 2f; // Thời gian chuyển đổi (giây)

    void Start()
    {
        if (virtualCamera == null)
        {
            Debug.LogError("Không tìm thấy Cinemachine Virtual Camera!");
            return;
        }
        // Đặt mục tiêu ban đầu
        virtualCamera.Follow = target1;
        virtualCamera.LookAt = target1;
        // Bắt đầu coroutine để thay đổi mục tiêu một lần
        StartCoroutine(SwitchTargetRoutine());
    }

    private IEnumerator SwitchTargetRoutine()
    {
        // Chờ thời gian timeSwitch
        yield return new WaitForSeconds(timeSwitch);
        // Chuyển đổi mục tiêu một lần
        isFollowingTarget1 = !isFollowingTarget1;
        virtualCamera.Follow = isFollowingTarget1 ? target1 : target2;
        virtualCamera.LookAt = isFollowingTarget1 ? target1 : target2;
    }
}