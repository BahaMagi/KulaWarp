using UnityEngine;

/**
 * The value of PickUps that do not take an instant effect (Currency.None) 
 * is accumulated in the LevelController according to their currency. 
 */
public enum Currency { None = -1, Points, Energy, Secret };

/**
* BaseClass that is attached to all objects which can be picked up by the player.
* They can either cause a one time effect or accumulate points in the LevelController. 
* Currecny.Points are used as "life" points. When the player dies the points 
* obtain in the currently level so far are substracted from the total points.
* If the total points fall below 0 its game over. 
* Currecny.Energy is used to activate the exit to the next level. 
* Currecny.Secret has yet to be implemented. 
* 
* It derives from ObjectBase and, thus, is automatically reset with the level. 
*/
public class PickUpBase : ObjectBase
{
    public Currency currency = Currency.None;
    public int      value    = 0;
    public Vector3  up       = Vector3.up;

    protected virtual void Awake()
    {
        // Register this object with the LevelController so it is reset on a restart.
        LevelController.lc.Register(this);
    }

    /**
     * This function is called when the object is being picked up by
     * the player. It scores its <value> of the corresponding 
     * currency at the LevelController and deactivates to become
     * invisible. 
     */
    protected virtual void OnTriggerEnter(Collider other)
    {
        gameObject.SetActive(false);
        LevelController.lc.Score(value, currency);
    }

    /**
     * When the level is reset, the Reset() method of all objects registered 
     * with LevelController.lc.Register() is called. 
     * The base version of the method simply activates the object again. 
     */
    public override void Reset()
    { gameObject.SetActive(true); }
}
