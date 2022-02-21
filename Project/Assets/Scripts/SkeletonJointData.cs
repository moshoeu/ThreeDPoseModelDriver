/******************************************************************
** 文件名:  SkeletonJointData.cs
** 版  权:  (C)
** 创建人:  moshoeu
** 日  期:  2022/02/09
** 描  述:  骨骼关节控制器

**************************** 修改记录 ******************************
** 修改人: 
** 日  期: 
** 描  述: 
*******************************************************************/

using System.Collections.Generic;
using UnityEngine;

namespace Framework
{
    public class SkeletonJointData 
    {
        /// <summary>
        /// 关节
        /// </summary>
        public class Joint
        {
            /// <summary>
            /// 关节位置
            /// </summary>
            public Vector3 m_Pos;

            /// <summary>
            /// 骨骼类型
            /// </summary>
            public HumanBodyBones m_BoneType;

            /// <summary>
            /// 骨骼节点
            /// </summary>
            public Transform m_BoneNode;
        }

        /// <summary>
        /// 关节数据输入
        /// </summary>
        public struct JointInput
        {
            /// <summary>
            /// 关节位置
            /// </summary>
            public Vector3 m_Pos;

            /// <summary>
            /// 骨骼类型
            /// </summary>
            public HumanBodyBones m_BoneType;
        }

        /// <summary>
        /// 根关节
        /// </summary>
        public TreeNode<Joint> m_RootJoint;

        /// <summary>
        /// 关节字典
        /// </summary>
        public Dictionary<HumanBodyBones, Joint> m_JointDict = 
            new Dictionary<HumanBodyBones, Joint>();

        /// <summary>
        /// 人形骨骼树
        /// </summary>
        private readonly TreeNode<HumanBodyBones> m_avatarBoneTree;

        /// <summary>
        /// 人形骨骼节点的字典
        /// </summary>
        private readonly Dictionary<HumanBodyBones, TreeNode<HumanBodyBones>> m_avatarBoneDict = 
            new Dictionary<HumanBodyBones, TreeNode<HumanBodyBones>>();

        public SkeletonJointData()
        {
            // 胯部
            var tips = new TreeNode<HumanBodyBones>(HumanBodyBones.Hips);

            // 腿部
            var leftUpperLeg = new TreeNode<HumanBodyBones>(HumanBodyBones.LeftUpperLeg, tips);
            var rightUpperLeg = new TreeNode<HumanBodyBones>(HumanBodyBones.RightUpperLeg, tips);
            var leftLowerLeg = new TreeNode<HumanBodyBones>(HumanBodyBones.LeftLowerLeg, leftUpperLeg);
            var rightLowerLeg = new TreeNode<HumanBodyBones>(HumanBodyBones.RightLowerLeg, rightUpperLeg);
            var leftFoot = new TreeNode<HumanBodyBones>(HumanBodyBones.LeftFoot, leftLowerLeg);
            var rightFoot = new TreeNode<HumanBodyBones>(HumanBodyBones.RightFoot, rightLowerLeg);
            var leftToes = new TreeNode<HumanBodyBones>(HumanBodyBones.LeftToes, leftFoot);
            var rightToes = new TreeNode<HumanBodyBones>(HumanBodyBones.RightToes, rightFoot);

            // 躯干
            var spine = new TreeNode<HumanBodyBones>(HumanBodyBones.Spine, tips);
            var chest = new TreeNode<HumanBodyBones>(HumanBodyBones.Chest, spine);
            var upperChest = new TreeNode<HumanBodyBones>(HumanBodyBones.UpperChest, chest);

            // 头部
            var neck = new TreeNode<HumanBodyBones>(HumanBodyBones.Neck, upperChest);
            var head = new TreeNode<HumanBodyBones>(HumanBodyBones.Head, neck);

            // 手部
            var leftShoulder = new TreeNode<HumanBodyBones>(HumanBodyBones.LeftShoulder, upperChest);
            var rightShoulder = new TreeNode<HumanBodyBones>(HumanBodyBones.RightShoulder, upperChest);
            var leftUpperArm = new TreeNode<HumanBodyBones>(HumanBodyBones.LeftUpperArm, leftShoulder);
            var rightUpperArm = new TreeNode<HumanBodyBones>(HumanBodyBones.RightUpperArm, rightShoulder);
            var leftLowerArm = new TreeNode<HumanBodyBones>(HumanBodyBones.LeftLowerArm, leftUpperArm);
            var rightLowerArm = new TreeNode<HumanBodyBones>(HumanBodyBones.RightLowerArm, rightUpperArm);
            var leftHand = new TreeNode<HumanBodyBones>(HumanBodyBones.LeftHand, leftLowerArm);
            var rightHand = new TreeNode<HumanBodyBones>(HumanBodyBones.RightHand, rightLowerArm);

            m_avatarBoneTree = tips;
            AddBoneNode2Dict(tips);
            
            // 添加骨骼的子节点到字典
            void AddBoneNode2Dict(TreeNode<HumanBodyBones> node)
            {
                m_avatarBoneDict.Add(node.m_Data, node);

                List<TreeNode<HumanBodyBones>> childs = node.m_Childs;
                foreach (TreeNode<HumanBodyBones> child in childs)
                {
                    AddBoneNode2Dict(child);
                }
            }
        }

        /// <summary>
        /// 初始化所有关节
        /// </summary>
        /// <param name="animator"></param>
        /// <param name="bones"></param>
        public void InitJoints(Animator animator, HumanBodyBones[] bones)
        {
            HashSet<HumanBodyBones> bonesSet = new HashSet<HumanBodyBones>(bones);

            TreeNode<Joint> hips = CreateJoint(HumanBodyBones.Hips);
            CreateChildJoint(hips);
            m_RootJoint = hips;

            // 尝试获取需要的骨骼节点
            Transform TryGetNeedBones(HumanBodyBones boneType)
            {
                Transform boneNode = null;

                if (bonesSet.Contains(boneType))
                {
                    boneNode = animator.GetBoneTransform(boneType);
                }

                return boneNode;
            }

            // 为关节节点设置子节点
            void CreateChildJoint(TreeNode<Joint> parent)
            {
                HumanBodyBones boneType = parent.m_Data.m_BoneType;
                TrySetChildJoint(parent, boneType);

                // 为子节点设置子节点
                foreach (TreeNode<Joint> child in parent.m_Childs)
                {
                    CreateChildJoint(child);
                }

                // 尝试设置子关节节点
                void TrySetChildJoint(TreeNode<Joint> parent, HumanBodyBones crtBoneType)
                {
                    TreeNode<HumanBodyBones> boneTypeNode = m_avatarBoneDict[crtBoneType];
                    List<TreeNode<HumanBodyBones>> boneTypeNodeChilds = boneTypeNode.m_Childs;

                    foreach (TreeNode<HumanBodyBones> child in boneTypeNodeChilds)
                    {
                        TreeNode<Joint> childJoint = CreateJoint(child.m_Data);
                        if (childJoint == null)
                        {
                            // 子节点不存在或者不是需要的子节点
                            // 查找该子节点的子节点
                            TrySetChildJoint(parent, child.m_Data);
                        }
                        else
                        {
                            childJoint.SetParent(parent);
                        }
                    }
                }
            }

            // 创建关节
            TreeNode<Joint> CreateJoint(HumanBodyBones boneType)
            {
                Transform boneNode = TryGetNeedBones(boneType);
                if (boneNode == null) return null;

                Joint joint = new Joint();
                joint.m_BoneType = boneType;
                joint.m_BoneNode = boneNode;
                joint.m_Pos = boneNode.position;

                m_JointDict.Add(boneType, joint);

                TreeNode<Joint> jointNode = new TreeNode<Joint>(joint);
                return jointNode;
            }

        }

        /// <summary>
        /// 更新所有关节
        /// </summary>
        /// <param name="inputs"></param>
        public void UpdateJoints(JointInput[] inputs)
        {
            // 更新关节位置数据
            for (int i = 0; i < inputs.Length; i++)
            {
                JointInput input = inputs[i];

                Joint joint;
                if (false == m_JointDict.TryGetValue(input.m_BoneType, out joint))
                {
                    Debug.LogError($"SkeletonJointData.cs : 更新关节[{input.m_BoneType}]失败！未在初始化关节时添加该关节！");
                    continue;
                }

                joint.m_Pos = input.m_Pos;
            }

            // 根节点位置变化
            m_RootJoint.m_Data.m_BoneNode.position = m_RootJoint.m_Data.m_Pos;

            UpdateRotate(m_RootJoint);

            // 更新旋转
            void UpdateRotate(TreeNode<Joint> jointNode)
            {
                CalculateJointRotate(jointNode);

                foreach (TreeNode<Joint> child in jointNode.m_Childs)
                {
                    UpdateRotate(child);
                }
            }

            // 计算关节旋转
            void CalculateJointRotate(TreeNode<Joint> jointNode)
            {
                if (jointNode.m_Childs.Count == 0)
                {
                    // 叶节点 暂时无法估计旋转
                    return;
                }

                Joint pJoint = jointNode.m_Data;
                Joint c0Joint = jointNode.m_Childs[0].m_Data;
                Transform parent = pJoint.m_BoneNode;
                Transform child0 = c0Joint.m_BoneNode;

                // 先参考第一个子节点的位置进行旋转
                Vector3 oldP2c0 = -parent.position + child0.position;
                Vector3 newP2c0 = -parent.position + c0Joint.m_Pos;
                Quaternion rotate = Quaternion.FromToRotation(oldP2c0, newP2c0);
                Quaternion initRotate = parent.rotation;
                parent.rotation = rotate * initRotate;

                if (jointNode.m_Childs.Count > 1)
                {
                    // 如果有多个子节点，再参考第二个子节点的位置进行旋转
                    // NOTE: 以p2c0为旋转轴，将第二个子节点旋转到目标位置
                    Joint c1Joint = jointNode.m_Childs[1].m_Data;
                    Transform child1 = c1Joint.m_BoneNode;

                    Vector3 oldP2c1 = -parent.position + child1.position;
                    Vector3 newDir2 = -parent.position + c1Joint.m_Pos;

                    // 将p2c1投影到p2c0上，求垂直于旋转轴的旋转圆的圆心
                    Vector3 project = Vector3.Project(oldP2c1, newP2c0.normalized);
                    Vector3 roundCenter = parent.position + project;

                    // 再把parent以p2c0为旋转轴 计算旋转角度 进行旋转
                    float angle = Vector3.SignedAngle(child1.position - roundCenter, 
                        c1Joint.m_Pos - roundCenter, newP2c0);
                    parent.RotateAround(parent.position, newP2c0, angle);
                }
            }
        }
    }
}