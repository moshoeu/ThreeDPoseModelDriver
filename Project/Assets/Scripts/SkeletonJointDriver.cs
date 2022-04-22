/******************************************************************
** 文件名:  SkeletonJointDriver.cs
** 版  权:  (C)
** 创建人:  moshoeu
** 日  期:  2022/02/09
** 描  述:  骨骼关节驱动

**************************** 修改记录 ******************************
** 修改人: 
** 日  期: 
** 描  述: 
*******************************************************************/

using System.Collections.Generic;
using UnityEngine;

namespace Framework
{
    public class SkeletonJointDriver : MonoBehaviour
    {
        private Animator m_animator;

        /// <summary>
        /// 关节数据
        /// </summary>
        private SkeletonJointData m_jointCtrl;

        /// <summary>
        /// 是否非法人形
        /// </summary>
        private bool m_isInvaildAvatar;

        /// <summary>
        /// 需要驱动的骨骼
        /// </summary>
        public HumanBodyBones[] m_Bones;

        public Dictionary<HumanBodyBones, SkeletonJointData.JointInput> m_Frame;

        /// <summary>
        /// 是否开启调试
        /// </summary>
        [SerializeField]
        private bool m_isDebug;

        void Start()
        {
            m_animator = GetComponent<Animator>();

            m_isInvaildAvatar = !(m_animator.avatar?.isHuman ?? false);
            if (m_isInvaildAvatar)
            {
                Debug.LogError($"SkeletonJointDriver.cs: 模型[{gameObject.name}]上的Avatar不是人形，无法进行骨骼驱动！");
                return;
            }

            m_jointCtrl = new SkeletonJointData();
            m_jointCtrl.InitJoints(m_animator, m_Bones);
        }

        void Update()
        {
            if (m_isInvaildAvatar)
            {
                return;
            }

            if (m_Frame == null) return;


            m_jointCtrl.UpdateJoints(new List<SkeletonJointData.JointInput>(m_Frame.Values).ToArray());
        
            if (m_isDebug)
            {
                DebugJoints();
            }
        }

        private void DebugJoints()
        {
            DrawJoint(m_jointCtrl.m_RootJoint);
            
            void DrawJoint(TreeNode<SkeletonJointData.Joint> jointNode)
            {
                var parentJoint = jointNode.m_Data;
                foreach (var child in jointNode.m_Childs)
                {
                    var childJoint = child.m_Data;
                    LineRenderer lineRender = GetLineRender(parentJoint.m_BoneType, childJoint.m_BoneType);
                    lineRender.positionCount = 2;
                    lineRender.SetPosition(0, parentJoint.m_Pos);
                    lineRender.SetPosition(1, childJoint.m_Pos);

                    DrawJoint(child);
                }
            }

            LineRenderer GetLineRender(HumanBodyBones parent, HumanBodyBones child)
            {
                var root = GameObject.Find("DebugJointRoot");
                if (root == null)
                {
                    root = new GameObject("DebugJointRoot");
                }

                string lineRenderName = $"Joint[{parent}]To[{child}]";
                Transform renderTrans = root.transform.Find(lineRenderName);
                if (renderTrans == null)
                {
                    var obj = new GameObject(lineRenderName);
                    obj.transform.SetParent(root.transform);
                    renderTrans = obj.transform;
                }

                LineRenderer result = renderTrans.GetComponent<LineRenderer>();
                if (result == null)
                {
                    result = renderTrans.gameObject.AddComponent<LineRenderer>();
                    result.startWidth = 0.02f;
                    result.endWidth = 0.02f;
                }
                return result;

            }
        }
    }
}