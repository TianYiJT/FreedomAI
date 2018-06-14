using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityECS;
using FreedomAI;

namespace FreedomAI
{
	public class AIWorld:ECSWorld
	{
		
		private StateControlSystem mStateControlSystem;
		private FrameCaptureSystem mFrameCaptureSystem;
		private FrameStatistics mFrameStatistics;
		private StateRecordSystem mStateRecordSystem;
		private AIMoveSystem mAIMoveSystem;
		private AIAnimationPlaySystem mAIAnimationPlaySystem;
		private EmotionControlSystem mEmotionControlSystem;
		private HurtControlSystem mHurtControlSystem;
		private AIDataRunSystem mAIDataRunSystem;
		private StrategyController mStrategyController;
		private StrategyCapturer mStrategyCapturer;
		private StrategyComputer mStrategyComputer;
		private SimpleAIComputeSystem mSimpleAIComputeSystem;
		private InfluenceMapUpdateSystem mInfluenceMapUpdateSystem;
		private InfluenceMapFlushSystem mInfluenceMapFlushSystem;
		private InfluenceMapShowSystem mInfluenceMapShowSystem;
		private GroupBehaviourSystem mGroupBehaviourSystem;
		private GroupStateUpdateSystem mGroupStateUpdateSystem;
		private GroupTransferSingleSystem mGroupTransferSingleSystem;
		private LODUpdateSystem mLODUpdateSystem;
		private ZoologySystem mZoologySystem;
		private AntPopulationSystem mAntPopulationSystem;

		// register all component default
		public override void registerAllComponent ()
		{
			base.registerAllComponent ();

			registerComponent (typeof(AIPoorComponent));
			registerComponent (typeof(BaseAIComponent));
			registerComponent (typeof(AIState));
			registerComponent (typeof(AIMove));
			registerComponent (typeof(AIAnimation));
			registerComponent (typeof(AIEmotion));
			registerComponent (typeof(HurtManager));
			registerComponent (typeof(HPComponent));
			registerComponent (typeof(AIStrategy));
			registerComponent (typeof(ObstacleComponent));
			registerComponent (typeof(SimpleAISet));
			registerComponent (typeof(InfluenceMap));
			registerComponent (typeof(InfluenceMapTrigger));
			registerComponent (typeof(GroupManager));
			registerComponent (typeof(AIGroupState));
			registerComponent (typeof(LODComponent));
			registerComponent (typeof(ZoologyComponent));
			registerComponent (typeof(AntPopulationComponent));
			registerComponent (typeof(RiskComponent));

		}

		// register all entity default
		public override void registerAllEntity ()
		{
			base.registerAllEntity ();
			registerEntity(SimpleAISetSingleton.getInstance ());
			registerEntity (InfluenceMapSingleton.getInstance());
			GameObject[] allCollision = GameObject.FindGameObjectsWithTag ("Collision");
			GameObject[] allBarrier = GameObject.FindGameObjectsWithTag ("Barrier");
			for (int i = 0; i < allCollision.Length; i++)
			{
				CollisionEntity tEntity = new CollisionEntity ();
				tEntity.mGameObject = allCollision [i];
				registerEntity (tEntity);
			}
			for (int i = 0; i < allBarrier.Length; i++) 
			{
				CollisionEntity tEntity = new CollisionEntity ();
				tEntity.mGameObject = allBarrier [i];
				registerEntity (tEntity);
			}
		//	registerEntity (GroupManagerEntity.getInstance());
			registerEntity (GroupManagerEntity.getInstance());
			registerEntity (AntPopulationEntity.getInstance());
		}


		// register all system
		public override void registerAllSystem ()
		{
			base.registerAllSystem ();

			mStateControlSystem = new StateControlSystem ();
			mFrameCaptureSystem = new FrameCaptureSystem ();
			mFrameStatistics = new FrameStatistics ();
			mStateRecordSystem = new StateRecordSystem ();
			mAIMoveSystem = new AIMoveSystem ();
			mAIAnimationPlaySystem = new AIAnimationPlaySystem ();
			mEmotionControlSystem = new EmotionControlSystem ();
			mHurtControlSystem = new HurtControlSystem ();
			mAIDataRunSystem = new AIDataRunSystem ();
			mStrategyController = new StrategyController ();
			mStrategyCapturer = new StrategyCapturer ();
			mStrategyComputer = new StrategyComputer ();
			mSimpleAIComputeSystem = new SimpleAIComputeSystem ();
			mInfluenceMapUpdateSystem = new InfluenceMapUpdateSystem ();
			mInfluenceMapFlushSystem = new InfluenceMapFlushSystem ();
			mInfluenceMapShowSystem = new InfluenceMapShowSystem ();
			mGroupBehaviourSystem = new GroupBehaviourSystem();
			mGroupStateUpdateSystem = new GroupStateUpdateSystem();
			mGroupTransferSingleSystem = new GroupTransferSingleSystem();
			mLODUpdateSystem = new LODUpdateSystem ();
			mZoologySystem = new ZoologySystem ();
			mAntPopulationSystem = new AntPopulationSystem ();

		
			registerSystem (mStateControlSystem);
			registerSystem (mFrameCaptureSystem);
			registerSystem (mFrameStatistics);
			registerSystem (mStateRecordSystem);
			registerSystem (mAIMoveSystem);
			registerSystem (mAIAnimationPlaySystem);
			registerSystem (mEmotionControlSystem);
			registerSystem (mHurtControlSystem);
			registerSystem (mAIDataRunSystem);
			registerSystem (mStrategyCapturer);
			registerSystem (mStrategyController);
			registerSystem (mStrategyComputer);
			registerSystem (mSimpleAIComputeSystem);
			registerSystem (mInfluenceMapUpdateSystem);
			registerSystem (mInfluenceMapFlushSystem);
			registerSystem (mInfluenceMapShowSystem);
			registerSystem (mGroupBehaviourSystem);
			registerSystem (mGroupStateUpdateSystem);
			registerSystem (mGroupTransferSingleSystem);
			registerSystem (mLODUpdateSystem);
			registerSystem (mZoologySystem);
			registerSystem (mAntPopulationSystem);

		}
			
	}
}


