using System;
using UnityEngine;

namespace Odisseia.Player
{
    /// <summary>
    /// Pequena mecânica de transformação de Circe: enquanto ativa, tinge o corpo,
    /// esconde a espada e desativa o combate (um porco não empunha espada). Reverte
    /// sozinha após <see cref="duration"/> ou imediatamente ao curar (erva de moly).
    /// </summary>
    [RequireComponent(typeof(PlayerCombat))]
    public class TransformationEffect : MonoBehaviour
    {
        [SerializeField] private float duration = 4f;
        [SerializeField] private Color transformedColor = new Color(0.9f, 0.6f, 0.7f);
        [SerializeField] private SpriteRenderer bodyRenderer;
        [SerializeField] private GameObject swordVisual;

        private PlayerCombat combat;
        private Color normalColor;
        private float timer;

        public bool IsTransformed => timer > 0f;
        public event Action<bool> TransformedChanged;

        private void Awake()
        {
            combat = GetComponent<PlayerCombat>();
            if (bodyRenderer != null)
            {
                normalColor = bodyRenderer.color;
            }
        }

        public void Transform()
        {
            bool wasTransformed = IsTransformed;
            timer = duration;

            if (!wasTransformed)
            {
                Apply();
            }
        }

        public void Cure()
        {
            if (!IsTransformed)
            {
                return;
            }

            timer = 0f;
            Revert();
        }

        private void Update()
        {
            if (timer <= 0f)
            {
                return;
            }

            timer -= Time.deltaTime;
            if (timer <= 0f)
            {
                Revert();
            }
        }

        private void Apply()
        {
            if (combat != null)
            {
                combat.enabled = false;
            }

            if (bodyRenderer != null)
            {
                bodyRenderer.color = transformedColor;
            }

            if (swordVisual != null)
            {
                swordVisual.SetActive(false);
            }

            TransformedChanged?.Invoke(true);
        }

        private void Revert()
        {
            if (combat != null)
            {
                combat.enabled = true;
            }

            if (bodyRenderer != null)
            {
                bodyRenderer.color = normalColor;
            }

            if (swordVisual != null)
            {
                swordVisual.SetActive(true);
            }

            TransformedChanged?.Invoke(false);
        }
    }
}
