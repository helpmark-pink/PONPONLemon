using UnityEngine;

namespace PONPONLemon.Effects
{
    public class DestroyEffect : MonoBehaviour
    {
        [Header("パーティクル")]
        [SerializeField] private ParticleSystem effectParticle;
        
        [Header("設定")]
        [SerializeField] private float lifetime = 1f;
        [SerializeField] private Color effectColor = Color.yellow;
        
        public void Play(Vector3 position, Color color)
        {
            transform.position = position;
            effectColor = color;
            
            if (effectParticle != null)
            {
                var main = effectParticle.main;
                main.startColor = color;
                effectParticle.Play();
            }
            
            Destroy(gameObject, lifetime);
        }
        
        public static DestroyEffect Create(GameObject prefab, Vector3 position, Color color, Transform parent = null)
        {
            if (prefab == null) return null;
            
            GameObject effectObj = Instantiate(prefab, position, Quaternion.identity, parent);
            DestroyEffect effect = effectObj.GetComponent<DestroyEffect>();
            
            if (effect != null)
            {
                effect.Play(position, color);
            }
            
            return effect;
        }
    }
}
