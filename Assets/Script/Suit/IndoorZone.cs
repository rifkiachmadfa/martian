using UnityEngine;

/// <summary>
/// Area di dalam habitat (Trigger). Selama pemain berada di dalam salah satu zona, pemain dianggap
/// "di dalam"; di luar semua zona, pemain dianggap di luar. Status ini dipakai SuitGate (tembok mati saat
/// pemain di luar) dan larangan melepas suit di luar. Oksigen suit sendiri berkurang selama suit dikenakan.
/// Boleh memakai beberapa zona (misalnya untuk bangunan berbentuk L); hitungannya dijumlahkan di SuitSystem.
/// Tepi zona sebaiknya sejajar dengan pintu keluar (SuitGate).
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class IndoorZone : MonoBehaviour
{
    private void Reset()
    {
        Collider2D zone = GetComponent<Collider2D>();
        if (zone != null) zone.isTrigger = true;
    }

    private void Awake()
    {
        Collider2D zone = GetComponent<Collider2D>();
        if (zone != null && !zone.isTrigger)
            Debug.LogWarning("IndoorZone: Collider2D harus Is Trigger agar zona bekerja.", this);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsPlayer(other)) return;

        SuitSystem suit = SuitSystem.Instance;
        if (suit != null) suit.NotifyEnteredIndoor();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!IsPlayer(other)) return;

        SuitSystem suit = SuitSystem.Instance;
        if (suit != null) suit.NotifyExitedIndoor();
    }

    private static bool IsPlayer(Collider2D other)
    {
        return other.GetComponentInParent<PlayerMovement>() != null;
    }

    private void OnDrawGizmos()
    {
        Collider2D zone = GetComponent<Collider2D>();
        if (zone == null) return;

        Gizmos.color = new Color(0.3f, 0.8f, 1f, 0.9f);
        Gizmos.DrawWireCube(zone.bounds.center, zone.bounds.size);
    }
}
