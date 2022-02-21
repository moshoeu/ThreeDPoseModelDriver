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
    private Animator m_animator;

    private SkeletonJointDriver m_driver;

    private List<Dictionary<HumanBodyBones, SkeletonJointData.JointInput>> frameInput
            = new List<Dictionary<HumanBodyBones, SkeletonJointData.JointInput>>();

    private bool m_isSample;

    // Start is called before the first frame update
    void Start()
    {
        m_animator = GetComponent<Animator>();
        m_driver = GetComponent<SkeletonJointDriver>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!m_isSample)
        {
            return;
        }

        Dictionary<HumanBodyBones, SkeletonJointData.JointInput> dict
            = new Dictionary<HumanBodyBones, SkeletonJointData.JointInput>();

        foreach (var bone in m_driver.m_Bones)
        {
            var pos = m_animator.GetBoneTransform(bone).position;
            SkeletonJointData.JointInput input = new SkeletonJointData.JointInput();
            input.m_BoneType = bone;
            input.m_Pos = pos;

            dict.Add(bone, input);
        }

        frameInput.Add(dict);
    }

    private void OnGUI()
    {
        if (GUILayout.Button("开始采样"))
        {
            m_isSample = true;
            frameInput.Clear();

            m_animator.enabled = true;
        }

        if (GUILayout.Button("结束采样"))
        {
            m_isSample = false;
            GetComponent<SkeletonJointDriver>().frameInput = frameInput;

            m_animator.enabled = false;
        }
    }
}

