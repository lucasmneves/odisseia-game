using System.Collections.Generic;
using UnityEngine;

namespace Odisseia.Systems
{
    /// <summary>
    /// Efeito de folha animada, tocado UMA vez numa posição e destruído ao fim — a arte do pack (golpe, slash, bloqueio,
    /// impacto, morte, coleta, poeira) no lugar dos quadradinhos do <see cref="VfxBurst"/>.
    ///
    /// Mesma linha do VfxBurst e do <see cref="SpriteAnimator"/>: um SpriteRenderer, nenhum ParticleSystem, nenhuma
    /// alocação por quadro, barato em WebGL. As folhas ficam em <c>Resources/Odisseia/Effects/</c> e são carregadas uma vez
    /// (cache estático); os quadros vêm ordenados pelo sufixo <c>_NN</c> do nome.
    ///
    /// Só apresentação: quem chama decide onde e quando; dano, alcance e tempo não passam por aqui.
    /// </summary>
    public class VfxSheet : MonoBehaviour
    {
        private const string Pasta = "Odisseia/Effects/";
        private static readonly Dictionary<string, Sprite[]> Cache = new Dictionary<string, Sprite[]>();

        private SpriteRenderer target;
        private Sprite[] frames;
        private float step;
        private float timer;
        private int frame;

        /// <summary>
        /// Toca a folha <paramref name="folha"/> em <paramref name="posicao"/>. Devolve false se a folha não existir —
        /// quem chama mantém o efeito antigo nesse caso, e nada some por falta de asset.
        /// </summary>
        public static bool Play(string folha, Vector3 posicao, bool espelhar = false, float fps = 16f, int ordem = 20)
        {
            Sprite[] quadros = Carregar(folha);
            if (quadros == null || quadros.Length == 0)
            {
                return false;
            }

            var go = new GameObject("Vfx_" + folha);
            go.transform.position = posicao;
            var vfx = go.AddComponent<VfxSheet>();
            vfx.target = go.AddComponent<SpriteRenderer>();
            vfx.target.sortingOrder = ordem;
            vfx.target.flipX = espelhar;
            vfx.frames = quadros;
            vfx.step = 1f / Mathf.Max(1f, fps);
            vfx.target.sprite = quadros[0];
            return true;
        }

        private static Sprite[] Carregar(string folha)
        {
            if (Cache.TryGetValue(folha, out Sprite[] quadros))
            {
                return quadros;
            }

            quadros = Resources.LoadAll<Sprite>(Pasta + folha);
            System.Array.Sort(quadros, (a, b) => string.CompareOrdinal(a.name, b.name));
            Cache[folha] = quadros;
            return quadros;
        }

        private void Update()
        {
            timer += Time.deltaTime;
            while (timer >= step)
            {
                timer -= step;
                frame++;
                if (frame >= frames.Length)
                {
                    Destroy(gameObject);
                    return;
                }

                target.sprite = frames[frame];
            }
        }
    }
}
