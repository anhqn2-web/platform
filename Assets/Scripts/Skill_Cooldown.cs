using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Skill_Cooldown : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Image cooldownOverlay;
    [SerializeField] private TextMeshProUGUI cooldownText;

    [Header("Skill Settings")]
    [SerializeField] private float cooldownTime = 10.0f; 
    private float currentCooldown = 0f;
    private bool isCooldown = false;

    private void Start()
    {
        // Ẩn lớp phủ và số đếm khi kỹ năng đã sẵn sàng
        cooldownOverlay.fillAmount = 0f;
        cooldownText.gameObject.SetActive(false);
    }

    private void Update()
    {
        // Kích hoạt bằng phím Q khi kỹ năng không trong thời gian hồi
        if (Input.GetKeyDown(KeyCode.Q) && !isCooldown)
        {
            UseSkill();
        }

        // Xử lý đếm ngược
        if (isCooldown)
        {
            currentCooldown -= Time.deltaTime;
            
            // Cập nhật vòng xoay (fillAmount chạy từ 1 về 0)
            cooldownOverlay.fillAmount = currentCooldown / cooldownTime;
            
            // Hiển thị thời gian với 1 chữ số thập phân (VD: 5.0, 3.2, 1.1)
            cooldownText.text = currentCooldown.ToString("F1"); 

            // Kết thúc hồi chiêu
            if (currentCooldown <= 0f)
            {
                isCooldown = false;
                currentCooldown = 0f;
                cooldownOverlay.fillAmount = 0f;
                cooldownText.gameObject.SetActive(false);
            }
        }
    }

    private void UseSkill()
    {
        isCooldown = true;
        currentCooldown = cooldownTime;
        
        // Bật lớp phủ đen và chữ đếm ngược
        cooldownOverlay.fillAmount = 1f;
        cooldownText.gameObject.SetActive(true);

        // TODO: Chèn logic tấn công của nhân vật (gọi animation, sinh sát thương) tại đây
        Debug.Log("Đã tung kỹ năng Q!");
    }
}