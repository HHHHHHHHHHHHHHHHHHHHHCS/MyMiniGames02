using UnityEngine;

namespace BoTWStasis.Scripts
{
	public class AttackStateController : StateMachineBehaviour
	{
		private static readonly int Attacking_ID = Animator.StringToHash("attacking");

		public bool enter;
		public bool exit;
		public bool setAttackBool;

		public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
		{
			if (!enter)
			{
				return;
			}

			StasisCharacter.instance.anim.SetBool(Attacking_ID, setAttackBool);
		}

		public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
		{
			if (!exit)
			{
				return;
			}

			StasisCharacter.instance.anim.SetBool(Attacking_ID, setAttackBool);
		}

		public override void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
		{
			if (!enter)
			{
				return;
			}

			StasisCharacter.instance.anim.SetBool(Attacking_ID, setAttackBool);
		}
	}
}