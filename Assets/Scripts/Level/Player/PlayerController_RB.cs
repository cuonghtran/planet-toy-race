/*********************************************************************************************************
 * Class players movement
 * -  Add this component to the players root object
 * *******************************************************************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityStandardAssets.CrossPlatformInput;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController_RB : MonoBehaviour
{
    public static PlayerController_RB Instance;

    // inspector variables
    [SerializeField, Tooltip("Player movement speed")]
    public float speed = 6f;
    [SerializeField, Tooltip("Player movement speed between worlds")]
    private float transferSpeed = 10.0f;
    [SerializeField, Tooltip("Player jump force")]
    private float jumpForce = 10.0f;
    [SerializeField, Tooltip("Player landing distance")]
    private float maxJumpHeight = 10.0f;
    [SerializeField, Tooltip("Landing Particles 1")]
    private ParticleSystem landingParticles1;
    [SerializeField, Tooltip("Landing Particles 2")]
    private ParticleSystem landingParticles2;
    [SerializeField, Tooltip("Distance from world to play landing particles (percentage of distance between worlds)"), Range(0.0f, 1.0f)]
    private float landDistance = 0.4f;
    [SerializeField, Tooltip("TakeOff Particles 1")]
    private ParticleSystem takeOffParticles1;
    [SerializeField, Tooltip("TakeOff Particles 2")]
    private ParticleSystem takeOffParticles2;
    [SerializeField, Tooltip("Explosion Particles")]
    private ParticleSystem explosionParticles;
    [SerializeField, Tooltip("Burning Particles 1")]
    private ParticleSystem burningParticles1;
    [SerializeField, Tooltip("Burning Particles 2")]
    private ParticleSystem burningParticles2;

    // privates
    private float rotateSense = 45;
    private float rotateModifier = 0.75f;
    //private List<GameObject> _worlds = new List<GameObject>();
    private int _currentWorld = 0;
    private int _prevWorld = 0;
    private Vector3 _moveDirection;
    private Rigidbody _playerRB;
    private Transform _playerMesh;
    private FakeGravityBody _worldGravity;
 
    // transfer
    private bool _transfering = false;
    private bool _isDead = false;
    private bool _landed = false;
    private float _worldDistance = 0;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    // Use this for initialization
    private void Start()
    {
        // set player details
        _playerRB = GetComponent<Rigidbody>();
        _playerMesh = transform.GetChild(0).transform;
        _worldGravity = GetComponent<FakeGravityBody>();
        // find worlds in scene
        //_worlds.AddRange(GameObject.FindGameObjectsWithTag("World"));
        _currentWorld = CurrentWorldIndex();
        // update player speed
        speed = SpeedUpdate();
        _moveDirection = new Vector3(0, 0, 1);
    }

    // Update is called once per frame
    private void Update()
    {
        // if changing worlds
        if (_transfering || _isDead || LevelManager.SharedInstance.gamePaused)
        {
            return;
        }

        // update move direction
        // _moveDirection = new Vector3(Input.GetAxisRaw("Horizontal") * rotateModifier, 0, 1).normalized;
        _moveDirection = new Vector3(CrossPlatformInputManager.GetAxis("Horizontal") * rotateModifier, 0, 1).normalized;

        // local rotation
        if (_moveDirection.x > 0)
        {
            transform.Rotate(0, rotateSense * Time.deltaTime, 0);
        }
        if (_moveDirection.x < 0)
        {
            transform.Rotate(0, -rotateSense * Time.deltaTime, 0);
        }

        // world transfer
        //if (Input.GetKeyDown("e"))
        //{
        //    Physics.IgnoreLayerCollision(8, 9, true);
        //    WorldTransfer();
        //}

        //// jump
        //if (Input.GetKeyDown("space"))
        //{
        //    Jump(false);
        //}

        // rotate player to face the right direction
        RotateForward();
    }

    // FixedUpdate is called every fixed framerate frame
    private void FixedUpdate()
    {
        // if changing worlds
        if (_transfering || _isDead)
        {
            return;
        }
        // update movement
        if (_moveDirection != Vector3.zero)
            _playerRB.MovePosition(_playerRB.position + transform.TransformDirection(_moveDirection * speed * Time.deltaTime));
    }

    /// <summary>
    /// Player Jump
    /// </summary>
    private void Jump(bool isTransferingWorld)
    {
        // get current jump height
        float jumpHeight = Vector3.Distance(WorldsManager.Instance.worlds[_currentWorld].transform.position, transform.position) - maxJumpHeight;
        // limit height to which jump is applied
        if (jumpHeight < maxJumpHeight)
        {
            // get direction of gravity
            Vector3 gravityDir = (WorldsManager.Instance.worlds[_currentWorld].transform.position - transform.position).normalized;
            // apply force against gravity
            _playerRB.AddForce(-gravityDir * jumpForce, ForceMode.Impulse);
            // play take off particle effect
            if (isTransferingWorld)
            {
                AudioManager.SharedInstance.Play("TakeOff");
                takeOffParticles1.Play();
                takeOffParticles2.Play();
            }
            else
            {
                landingParticles1.Play();
                landingParticles2.Play();
            }
        }
    }

    /// <summary>
    /// Initialise player transfer between worlds
    /// </summary>
    public void WorldTransfer()
    {
        if (_isDead || _transfering)
            return;

        // initialise planet transfer
        _transfering = true;

        LevelManager.SharedInstance.UpgradeScore();
        AudioManager.SharedInstance.Stop("Engine2");

        // launch player
        Jump(true);
        // disconnect gravity
        _worldGravity.Attractor = null;
        // increment world ID
        if (_currentWorld + 1 >= WorldsManager.Instance.worlds.Count)
        {
            _prevWorld = _currentWorld;
            _currentWorld = 0;
        }
        else
        {
            _prevWorld = _currentWorld;
            _currentWorld++;
        }
        LevelManager.SharedInstance.isTransfering = true;
        // distance between _worlds
        _worldDistance = Vector3.Distance(WorldsManager.Instance.worlds[_prevWorld].transform.position, WorldsManager.Instance.worlds[_currentWorld].transform.position) -
                                        (WorldsManager.Instance.worlds[_prevWorld].GetComponent<FakeGravity>().WorldSize + WorldsManager.Instance.worlds[_currentWorld].GetComponent<FakeGravity>().WorldSize);
        // start change worlds coroutine
        StartCoroutine(ChangeWorlds());
    }
    
    /// <summary>
    /// Corountine that controls changing worlds
    /// </summary>
    /// <returns></returns>
    private IEnumerator ChangeWorlds()
    {
        while (_transfering)
        {
            // move to travel height
            yield return StartCoroutine(TakeOff());
            // rotate to target
            yield return StartCoroutine(RotateToTarget());
            // travel to target
            yield return StartCoroutine(TravelToTarget());
        }
    }

    /// <summary>
    /// Player take off coroutine
    /// </summary>
    /// <returns></returns>
    private IEnumerator TakeOff()
    {
        bool done = false;

        while (!done)
        {
            float jumpDistance = Vector3.Distance(WorldsManager.Instance.worlds[_prevWorld].transform.position, transform.position);
            // transfer once player reaches max jump height
            if (jumpDistance > (maxJumpHeight * 1.75f))
            {
                // reduce velocity
                if (_playerRB.velocity.magnitude > 2f)
                {
                    _playerRB.velocity -= (transform.up * 8f) * Time.deltaTime;
                }
                else
                {
                    _playerRB.velocity = Vector3.zero;
                    // finish coroutine
                    done = true;
                }
            }
            yield return null;
        }
    }

    /// <summary>
    /// Coroutine to rotate player to target world
    /// </summary>
    /// <returns></returns>
    private IEnumerator RotateToTarget()
    {
        bool done = false;

        while(!done)
        {
            // set move direction
            _moveDirection = (WorldsManager.Instance.worlds[_currentWorld].transform.position - transform.position).normalized;
            // rotate player
            transform.rotation = Quaternion.Slerp(
                        transform.rotation,
                        Quaternion.FromToRotation(-Vector3.up, _moveDirection),
                        Time.deltaTime * 1.75f
                        );
            // check if rotation is complete
            if (Vector3.Distance(_moveDirection, -transform.up) <= 0.1f)
            {
                AudioManager.SharedInstance.Play("TakeOff");
                // play take off particle effect
                takeOffParticles1.Play();
                takeOffParticles2.Play();
                // finish coroutine
                done = true;
            }
            yield return null;
        }
    }

    /// <summary>
    /// Coroutine to move player to new world and land
    /// </summary>
    /// <returns></returns>
    private IEnumerator TravelToTarget()
    {
        bool done = false;

        while(!done)
        {
            // get direction to world and move player
            _moveDirection = (WorldsManager.Instance.worlds[_currentWorld].transform.position - transform.position).normalized;
            // get distance from new world
            float distance = Vector3.Distance(WorldsManager.Instance.worlds[_currentWorld].transform.position, transform.position);
            // start landing rotation before hitting atmosphere
            if (distance < (_worldDistance * landDistance) + 5)
            {
                // rotate to land
                transform.rotation = Quaternion.Slerp(
                                                    transform.rotation,
                                                    Quaternion.FromToRotation(-Vector3.up, _moveDirection),
                                                    Time.deltaTime * 1.2f
                                                    );
                // move slower now closer to world
                _playerRB.MovePosition(_playerRB.position + (_moveDirection * (transferSpeed * 0.5f) * Time.deltaTime));
            }
            else
            {
                // apply normal travel speed
                _playerRB.MovePosition(_playerRB.position + (_moveDirection * transferSpeed * Time.deltaTime));
            }
            // check if entering atmosphere
            if (distance < (_worldDistance * landDistance))
            {
                // if landed then arrived at new world
                if (_landed)
                {
                    // set new attractor
                    _worldGravity.Attractor = WorldsManager.Instance.worlds[_currentWorld].GetComponent<FakeGravity>();
                    StartCoroutine(_worldGravity.Attractor.ActivateTakeOffPad(LevelManager.SharedInstance.timeToActivateTakeOffPad));
                    // reset transfer state
                    ResetState();
                    // finish coroutine
                    done = true;
                }
                else
                {
                    // play landing particles
                    landingParticles1.Play();
                    landingParticles2.Play();
                }
            }
            yield return null;
        }
    }

    /// <summary>
    /// Rotate player to face direction of movement
    /// </summary>
    private void RotateForward()
    {
        Vector3 dir = _moveDirection;
        // calculate angle and rotation
        float angle = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
        Quaternion targetRotation = Quaternion.AngleAxis(angle, Vector3.up);
        // only update rotation if direction greater than zero
        if (Vector3.Magnitude(dir) > 0.0f)
        {
            _playerMesh.localRotation = Quaternion.Lerp(_playerMesh.localRotation, targetRotation, 4f * Time.deltaTime);
        }
    }

    /// <summary>
    /// Update player speed based on world
    /// </summary>
    private float SpeedUpdate()
    {
        float newSpeed = speed;
        // update speed value
        if (_worldGravity.Attractor.gameObject.name == "PlaneWorld")
        {
            newSpeed = speed / 2;
        }
        // return result
        return newSpeed;
    }

    /// <summary>
    /// Get current world player is on
    /// </summary>
    private int CurrentWorldIndex()
    {
        int worldIndex = 0;
        // get name of current world player is attracted to
        string worldName = _worldGravity.Attractor.gameObject.name;
        // iterate through list of worlds
        int count = WorldsManager.Instance.worlds.Count;
        for (int i = 0; i < count; i++)
        {
            // check if world in list has same name as curretn attractor
            if (worldName == WorldsManager.Instance.worlds[i].name)
            {
                worldIndex = i;
            }
        }
        // return result
        return worldIndex;
    }

    /// <summary>
    /// Reset bools used for world transfer
    /// </summary>
    private void ResetState()
    {
        LevelManager.SharedInstance.isTransfering = false;
        _transfering = false;
        _landed = false;
    }

    /// <summary>
    /// Called when player enters a collider
    /// </summary>
    /// <param name="collision"></param>
    private void OnCollisionEnter(Collision collision)
    {
        // if player transfering between worlds and has collided with a world
        if(_transfering && collision.transform.CompareTag("World"))
        {
            // player landed on world
            AudioManager.SharedInstance.Play("Engine2");
            _landed = true;
            landingParticles1.Stop();
            landingParticles2.Stop();
            StartCoroutine(StopIgnoreCollision());
        }
    }

    IEnumerator StopIgnoreCollision()
    {
        yield return new WaitForSeconds(2);
        Physics.IgnoreLayerCollision(8, 9, false);
    }

    public IEnumerator DestroyPlayer()
    {
        _isDead = true;
        LevelManager.SharedInstance.gamePaused = true;
        //_playerRB.velocity = Vector3.zero;
        _moveDirection = Vector3.zero;
        yield return PlayDeathSequence();
        yield return new WaitForSeconds(0.25f);
        LevelManager.SharedInstance.AfterDeath();
    }

    IEnumerator PlayDeathSequence()
    {
        explosionParticles.Play();
        yield return new WaitForSeconds(0.5f);
        burningParticles1.Play();
        burningParticles2.Play();
    }
}
