using UnityEngine;

namespace Odisseia.Core
{
    /// <summary>
    /// Contexto de um golpe: de onde veio e se pode ser bloqueado.
    ///
    /// Existe porque <see cref="HealthSystem.TakeDamage(int)"/> sozinho não diz nada
    /// sobre direção, e o escudo precisa saber se o ataque veio de frente. O valor
    /// padrão (<see cref="Environmental"/>) representa dano sem origem — queda, poço,
    /// afogamento, canto das sereias —, que por definição não dá para bloquear.
    /// </summary>
    public readonly struct DamageInfo
    {
        /// <summary>Posição de onde partiu o golpe (só válida se <see cref="HasSource"/>).</summary>
        public readonly Vector2 Source;

        public readonly bool HasSource;

        /// <summary>
        /// Se o golpe pode ser bloqueado por escudo. Ataques comuns são bloqueáveis;
        /// ataques especiais podem ser marcados como não bloqueáveis pelo atacante.
        /// </summary>
        public readonly bool Blockable;

        public DamageInfo(Vector2 source, bool blockable = true)
        {
            Source = source;
            HasSource = true;
            Blockable = blockable;
        }

        /// <summary>Dano sem origem conhecida — nunca bloqueável.</summary>
        public static DamageInfo Environmental => default;
    }

    /// <summary>
    /// Componente capaz de reduzir o dano antes dele chegar ao <see cref="HealthSystem"/>.
    /// O HealthSystem procura um destes no mesmo GameObject e continua sem saber o que é
    /// um escudo — quem não tiver mitigador nenhum se comporta exatamente como antes.
    /// </summary>
    public interface IDamageMitigator
    {
        /// <summary>Devolve o dano já reduzido (pode ser o mesmo valor).</summary>
        int Mitigate(int amount, DamageInfo info);
    }
}
