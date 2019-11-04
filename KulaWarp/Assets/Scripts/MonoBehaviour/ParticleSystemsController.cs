using System.Collections.Generic;
using UnityEngine;

public class ParticleSystemsController: MonoBehaviour
{
    public static ParticleSystemsController psc;

    List<PooledPS> pool;

    // Base Classes MonoBehaviour:

    void Awake()
    {
        // Make this a public singelton
        if (psc == null) psc = this;
        else if (psc != this) Destroy(gameObject);

        pool = new List<PooledPS>();
    }

    // ParticleSystemsController:

    public void PlayPS(int ID, Vector3 pos, Vector3 up)
    {
        ParticleSystem ps = pool[ID].getInstance();

        // Move particle system to target position and rotate according to worlds up direction.
        ps.transform.rotation  = Quaternion.FromToRotation(Vector3.up, up);
        ps.transform.position  = pos;

        // Apply any transforms given in the prefab.
        ps.transform.Translate(pool[ID].offset, Space.Self);
        ps.transform.rotation *= pool[ID].orientation;

        ps.Play();
    }

    public int AddPrefabtoPool(ParticleSystem ps, int poolSize = 1)
    {
        PooledPS pps = new PooledPS(ps, poolSize);

        // Only add <ps> if it it not already in the pool. 
        if (!pool.Contains(pps))
            pool.Add(pps);

        return pool.IndexOf(pps);
    }

    public int GetPrefabID(ParticleSystem ps)
    {
        PooledPS pps = new PooledPS(ps);

        // Return -1 if the prefab is not pooled yet. 
        if (!pool.Contains(pps))
            return -1;

        return pool.IndexOf(pps);
    }

    // Local Classes:

    private class PooledPS
    {
        ParticleSystem ps;
        List<ParticleSystem> instances;
        int poolSize = 1, nextIndex = 0;

        public Vector3 offset;
        public Quaternion orientation;

        public PooledPS(ParticleSystem ps, int poolSize = 1)
        {
            this.ps       = ps;
            this.poolSize = poolSize;
            offset        = ps.transform.position;
            orientation   = ps.transform.rotation;
            instances     = new List<ParticleSystem>(poolSize);
            for (int i = 0; i < poolSize; i++)
            {
                instances.Add(Instantiate(ps, new Vector3(0, 0, 0), Quaternion.identity));
                instances[i].transform.SetParent(psc.transform);
            }
        }

        public ParticleSystem getInstance()
        {
            ParticleSystem ret;
            if (instances[nextIndex].isPlaying)
            {
                instances.Insert(nextIndex, Instantiate(ps, new Vector3(0, 0, 0), Quaternion.identity));
                poolSize++;
            }
            ret = instances[nextIndex];
            nextIndex = (++nextIndex) % poolSize;

            return ret;
        }

        public static bool operator ==(PooledPS pps1, PooledPS pps2)
        { return pps1.ps == pps2.ps; }

        public static bool operator !=(PooledPS pps1, PooledPS pps2)
        { return pps1.ps != pps2.ps; }

        public override bool Equals(object obj)
        {
            if (obj.GetType() == typeof(PooledPS))
                return this == (obj as PooledPS);

            return base.Equals(obj);
        }

        public override int GetHashCode()
        { return ps.GetHashCode(); }
    }
}
