/******************************************************************
** 文件名:  JointPosSampler.cs
** 版  权:  (C)
** 创建人:  moshoeu
** 日  期:  2022//
** 描  述:  

**************************** 修改记录 ******************************
** 修改人: 
** 日  期: 
** 描  述: 
*******************************************************************/

using Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JointPosSampler : MonoBehaviour
{
    [SerializeField]
    private Animator m_animator;

    [SerializeField]
    private SkeletonJointDriver m_driver;

    [SerializeField]
    private Vector3 m_rootOffset;

    private bool m_isSample;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

        Dictionary<HumanBodyBones, SkeletonJointData.JointInput> dict
            = new Dictionary<HumanBodyBones, SkeletonJointData.JointInput>();

        foreach (var bone in m_driver.m_Bones)
        {
            var pos = m_animator.GetBoneTransform(bone).position;

            pos += m_rootOffset;
            

            SkeletonJointData.JointInput input = new SkeletonJointData.JointInput();
            input.m_BoneType = bone;
            input.m_Pos = pos;


            dict.Add(bone, input);
        }

        m_driver.m_Frame = dict;
    }

}

