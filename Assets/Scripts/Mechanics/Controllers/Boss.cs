using Assets.Scripts;
using NeonLadder.Events;
using NeonLadder.Mechanics.Stats;
using System.Collections;
using UnityEngine;
using static NeonLadder.Core.Simulation;

namespace NeonLadder.Mechanics.Controllers
{
    public class Boss : MonoBehaviour
    {
        public AudioSource audioSource;
        public AudioClip ouchAudio;
        public AudioClip jumpAudio;
        public AudioClip landAudio;

        public Health health;
        public Stamina stamina;

        void OnCollisionEnter2D(Collision2D collision)
        {

            //switch (collision.collider.tag)
            //{
            //    case "Weapon":
            //        var wbc = Schedule<WeaponBossCollision>();
            //        wbc.player = collision.gameObject.GetComponent<ProtagonistController>();
            //        wbc.boss = this;
            //        wbc.collider = collision;

            //        break;
            //    case "Player":
            //        var pbc = Schedule<PlayerBossCollision>();
            //        pbc.player = collision.gameObject.GetComponent<ProtagonistController>(); ;
            //        pbc.boss = this;
            //        pbc.collider = collision;
            //        break;
            //    default:
            //        break;
            //}
        }


        void Awake()
        {
            audioSource = GetComponent<AudioSource>();
        }


        protected void Update()
        {

        }
    }
}