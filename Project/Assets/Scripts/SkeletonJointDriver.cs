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
        // 驱动模式
        public enum DriveMode
        {
            /// <summary>
            /// 不驱动
            /// </summary>
            DontDrive,

            /// <summary>
            /// 立即更新
            /// </summary>
            Immediately,

            /// <summary>
            /// 角速度
            /// </summary>
            AngularVelocity
        }

        private Animator m_animator;

        /// <summary>
        /// 是否非法人形
        /// </summary>
        private bool m_isInvaildAvatar;

        public DriveMode m_DriveMode;

        [SerializeField]
        private float m_angularVelocity;

        /// <summary>
        /// 需要驱动的骨骼
        /// </summary>
        public HumanBodyBones[] m_Bones;

        /// <summary>
        /// 是否开启调试
        /// </summary>
        [SerializeField]
        private bool m_isDebug;

        /// <summary>
        /// 关节数据
        /// </summary>
        public SkeletonJointData m_JointsData;


        public Dictionary<HumanBodyBones, SkeletonJointData.JointInput> Frame
        {
            private get;
            set;
        }

        void Start()
        {
            m_animator = GetComponent<Animator>();

            m_isInvaildAvatar = !(m_animator.avatar?.isHuman ?? false);
            if (m_isInvaildAvatar)
            {
                Debug.LogError($"SkeletonJointDriver.cs: 模型[{gameObject.name}]上的Avatar不是人形，无法进行骨骼驱动！");
                return;
            }

            m_JointsData = new SkeletonJointData();
            m_JointsData.InitJoints(m_animator, m_Bones);
        }

        void Update()
        {
            if (m_isInvaildAvatar)
            {
                return;
            }

            if (Frame == null) return;

            m_JointsData.CalcJoints(new List<SkeletonJointData.JointInput>(Frame.Values).ToArray());
            TryDriveJoints();


            if (m_isDebug)
            {
                DebugJoints();
            }
        }

        private void DebugJoints()
        {
            DrawJoint(m_JointsData.RootJoint);
            
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

        /// <summary>
        /// 立即驱动骨骼
        /// </summary>
        private void DriveJointsImmediately()
        {
            var rootJoint = m_JointsData.RootJoint;
            DriveJoint(rootJoint);

            // 更新旋转
            void DriveJoint(TreeNode<SkeletonJointData.Joint> jointNode)
            {
                var joint = jointNode.m_Data;
                joint.m_BoneNode.rotation = joint.m_Rotation;

                foreach (TreeNode<SkeletonJointData.Joint> child in jointNode.m_Childs)
                {
                    DriveJoint(child);
                }
            }
        }

        /// <summary>
        /// 驱动骨骼
        /// </summary>
        private void DriveJointsSmooth()
        {
            var rootJoint = m_JointsData.RootJoint;
            DriveJoint(rootJoint);

            // 更新旋转
            void DriveJoint(TreeNode<SkeletonJointData.Joint> jointNode)
            {
                var joint = jointNode.m_Data;
                //joint.m_BoneNode.rotation = joint.m_Rotation;

                var deltaRotation = Quaternion.RotateTowards(joint.m_BoneNode.rotation, joint.m_Rotation, m_angularVelocity * Time.deltaTime);
                joint.m_BoneNode.rotation = deltaRotation;

                foreach (TreeNode<SkeletonJointData.Joint> child in jointNode.m_Childs)
                {
                    DriveJoint(child);
                }
            }
        }

        /// <summary>
        /// 试图驱动骨骼
        /// </summary>
        private void TryDriveJoints()
        {
            switch (m_DriveMode)
            {
                case DriveMode.Immediately:
                    {
                        DriveJointsImmediately();
                        break;
                    }
                case DriveMode.AngularVelocity:
                    {
                        DriveJointsSmooth();
                        break;
                    }
                default:break;
            }
        }
    }
}