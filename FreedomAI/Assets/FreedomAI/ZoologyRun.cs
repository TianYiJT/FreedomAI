using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityECS;
using FreedomAI;


public class ZoologyRun : MonoBehaviour
{

	public GameObject mGlass;
	public GameObject glass_leftdown;
	public GameObject glass_rightup;
	public GameObject mCow;
	public GameObject cow_leftdown;
	public GameObject cow_rightup;
	public GameObject mTree;
	public GameObject tree_leftdown;
	public GameObject tree_rightup;

	private GAPopulation GlassPopulation = new GAPopulation ();
	private GAPopulation CowPopulation = new GAPopulation ();
	private GAPopulation TreePopulation = new GAPopulation ();

	public int mMaxGrass;

	public int mMaxCow;

	public int mMaxTree;


	void Start ()
	{
		GlassPopulation.AddMySelf ();
		CowPopulation.AddMySelf ();
		TreePopulation.AddMySelf ();



		GlassPopulation.mBreedCycle = 15.0f;
		GlassPopulation.mBreedor = DefaultGAFunc.SinglePointInsertBreedor;
		GlassPopulation.mDyingRate = 0.8f;
		GlassPopulation.mIncreaseRate =1.0f/0.6f;
		GlassPopulation.mMaxCount = mMaxGrass;
		GlassPopulation.mMutator = DefaultGAFunc.SimpleMutator;
		GlassPopulation.mMutatorRate = 0.005f;
		GlassPopulation.mObjectType = typeof(glassObject);
		glassObject.mpos_leftdown = glass_leftdown.transform.position;
		glassObject.mpos_rightup = glass_rightup.transform.position;
		GlassPopulation.mPlayer = GameObject.FindGameObjectWithTag ("Player");
		GlassPopulation.mPrefab = mGlass;
		GlassPopulation.mRTObjectDestroyer = new GlassDestroyer ();
		GlassPopulation.mRTObjectJudger = new GlassJudger ();
		GlassPopulation.mRTObjectRunner = new GlassRunner ();
		GlassPopulation.mRTType = "Glass";
		GlassPopulation.mSelector = DefaultGAFunc.RouletteSelector;
		GlassPopulation.mSimpleAIRePairer = new GlassRePairer ();

		CowPopulation.mBreedCycle = 25.0f;
		CowPopulation.mBreedor = DefaultGAFunc.SinglePointInsertBreedor;
		CowPopulation.mDyingRate = 0.8f;
		CowPopulation.mIncreaseRate = 1 / 0.7f;
		CowPopulation.mMaxCount = mMaxCow;
		CowPopulation.mMutator = DefaultGAFunc.SimpleMutator;
		CowPopulation.mMutatorRate = 0.005f;
		CowPopulation.mObjectType = typeof(CowObject);
		CowObject.mpos_leftdown = cow_leftdown.transform.position;
		CowObject.mpos_rightup = cow_rightup.transform.position;
		CowPopulation.mPlayer = GameObject.FindGameObjectWithTag ("Player");
		CowPopulation.mPrefab = mCow;
		CowPopulation.mRTObjectDestroyer = new CowDestroyer ();
		CowPopulation.mRTObjectJudger = new CowJudger ();
		CowPopulation.mRTObjectRunner = new CowRunner ();
		CowPopulation.mRTType = "Cow";
		CowPopulation.mSelector = DefaultGAFunc.RouletteSelector;
		CowPopulation.mSimpleAIRePairer = new CowRePairer ();


		TreePopulation.mBreedCycle = 25.0f;
		TreePopulation.mBreedor = DefaultGAFunc.SinglePointInsertBreedor;
		TreePopulation.mDyingRate = 0.85f;
		TreePopulation.mIncreaseRate = 1 / 0.75f;
		TreePopulation.mMaxCount = mMaxTree;
		TreePopulation.mMutator = DefaultGAFunc.SimpleMutator;
		TreePopulation.mObjectType = typeof(TreeObject);
		TreeObject.mpos_leftdown = tree_leftdown.transform.position;
		TreeObject.mpos_rightup = tree_rightup.transform.position;
		TreePopulation.mPlayer = GameObject.FindGameObjectWithTag ("Player");
		TreePopulation.mPrefab = mTree;
		TreePopulation.mRTObjectDestroyer = new CowDestroyer ();
		TreePopulation.mRTObjectJudger = new TreeJudger ();
		TreePopulation.mRTObjectRunner = new TreeRunner ();
		TreePopulation.mRTType = "Tree";
		TreePopulation.mSelector = DefaultGAFunc.RouletteSelector;
		TreePopulation.mSimpleAIRePairer = new TreeRePairer ();

		GAPopulationManager.getInstance ().Start ();
	}
	

	void Update ()
	{
		//CowPopulation.
	}
}
