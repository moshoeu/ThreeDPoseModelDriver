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

            /// <summary>
            /// 初始旋转
            /// </summary>
            public Quaternion m_InitInverseRotate;
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

            (Vector3 fwd, Vector3 up) fwdResult = GetForward(true);
            InitJointInverseRotate(hips);

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

            // 初始化节点逆旋转
            void InitJointInverseRotate(TreeNode<Joint> jointNode)
            {
                Joint pJoint = jointNode.m_Data;
                Transform parent = pJoint.m_BoneNode;
                var gpNode = jointNode.Parent;

                Vector3 oldFwd, oldUp;

                if (jointNode.m_Childs.Count == 0)
                {
                    // 特殊处理 没有子节点时 和父节点旋转保持一致
                    if (gpNode == null)
                    {
                        oldUp = hips.m_Data.m_BoneNode.up;
                        oldFwd = hips.m_Data.m_BoneNode.forward;
                    }
                    else
                    {
                        var gpJoint = gpNode.m_Data;

                        oldUp = gpJoint.m_BoneNode.up;
                        oldFwd = gpJoint.m_BoneNode.forward;
                    }
                }
                else
                {
                    Joint c0Joint = jointNode.m_Childs[0].m_Data;
                    Vector3 oldC0Pos = c0Joint.m_BoneNode.position;
                    // 先参考第一个子节点的位置进行旋转
                    Vector3 oldP2c0 = -parent.position + oldC0Pos;

                    if (jointNode.m_Childs.Count >= 2)
                    {
                        Joint c1Joint = jointNode.m_Childs[1].m_Data;

                        Vector3 oldC1Pos = c1Joint.m_BoneNode.position;
                        Vector3 oldP2c1 = -parent.position + oldC1Pos;

                        oldFwd = Vector3.Cross(oldP2c0, oldP2c1).normalized;
                        oldUp = oldP2c0.normalized;

                    }
                    else
                    {
                        if (gpNode == null)
                        {
                            oldUp = hips.m_Data.m_BoneNode.up;
                        }
                        else
                        {
                            var gpJoint = gpNode.m_Data;
                            oldUp = (parent.position - gpJoint.m_BoneNode.position).normalized;
                        }

                        oldFwd = oldP2c0.normalized;
                    }
                }



                var oldRotate = Quaternion.LookRotation(oldFwd, oldUp);
                var oldRotateInverse = Quaternion.Inverse(oldRotate);

                pJoint.m_InitInverseRotate = oldRotateInverse * parent.rotation;

                foreach (TreeNode<Joint> child in jointNode.m_Childs)
                {
                    InitJointInverseRotate(child);
                }
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
            (Vector3 fwd, Vector3 up) fwdResult = GetForward(false);

            UpdateRotate(m_RootJoint);

            // 更新旋转
            void UpdateRotate(TreeNode<Joint> jointNode)
            {
                CalculateJointRotateEx(jointNode);

                foreach (TreeNode<Joint> child in jointNode.m_Childs)
                {
                    UpdateRotate(child);
                }
            }

            // 计算关节旋转
            void CalculateJointRotateEx(TreeNode<Joint> jointNode)
            {
                Joint pJoint = jointNode.m_Data;
                Transform parent = pJoint.m_BoneNode;
                Vector3 newFwd, newUp;

                var gpNode = jointNode.Parent;

                if (jointNode.m_Childs.Count == 0)
                {
                    // 特殊处理 没有子节点时 和父节点旋转保持一致
                    if (gpNode == null)
                    {
                        newUp = fwdResult.up;
                        newFwd = fwdResult.fwd;
                    }
                    else
                    {
                        var gpJoint = gpNode.m_Data;

                        newUp = gpJoint.m_BoneNode.up;
                        newFwd = gpJoint.m_BoneNode.forward;
                    }
                }
                else 
                {
                    Joint c0Joint = jointNode.m_Childs[0].m_Data;
                    Vector3 newC0Pos = c0Joint.m_Pos;

                    // 先参考第一个子节点的位置进行旋转
                    Vector3 newP2c0 = -pJoint.m_Pos + newC0Pos;

                    if (jointNode.m_Childs.Count == 1)
                    {
                        if (gpNode == null)
                        {
                            newUp = fwdResult.up;
                        }
                        else
                        {
                            var gpJoint = gpNode.m_Data;
                            newUp = (pJoint.m_Pos - gpJoint.m_Pos).normalized;
                        }

                        newFwd = newP2c0.normalized;
                    }
                    else
                    {
                        Joint c1Joint = jointNode.m_Childs[1].m_Data;
                        Vector3 newC1Pos = c1Joint.m_Pos;
                        Vector3 newP2c1 = -pJoint.m_Pos + newC1Pos;

                        // TODO: 若父节点是子节点取中点算出来的 这里叉乘会得零向量出错 要对节点做处理
                        newFwd = Vector3.Cross(newP2c0, newP2c1).normalized;
                        newUp = newP2c0.normalized;
                    }
                }

                var newRotate = Quaternion.LookRotation(newFwd, newUp);

                Quaternion initRotate = pJoint.m_InitInverseRotate;
                parent.rotation = newRotate * initRotate;
            }
        }

        /// <summary>
        /// 获取面朝方向
        /// </summary>
        /// <param name="keyPointCollection"></param>
        /// <returns></returns>
        public (Vector3 fwd, Vector3 up) GetForward(bool isTransformPos)
        {
            var rHips = m_JointDict[HumanBodyBones.RightUpperLeg];
            var lHips = m_JointDict[HumanBodyBones.LeftUpperLeg];

            var rShldr = m_JointDict[HumanBodyBones.RightUpperArm];
            var lShldr = m_JointDict[HumanBodyBones.LeftUpperArm];

            Vector3 mShldrPos, rVec, lVec;

            if (isTransformPos)
            {
                mShldrPos = (rShldr.m_BoneNode.position + lShldr.m_BoneNode.position) / 2f;

                rVec = rHips.m_BoneNode.position - mShldrPos;
                lVec = lHips.m_BoneNode.position - mShldrPos;
            }
            else
            {
                mShldrPos = (rShldr.m_Pos + lShldr.m_Pos) / 2f;

                rVec = rHips.m_Pos - mShldrPos;
                lVec = lHips.m_Pos - mShldrPos;
            }


            var forward = Vector3.Cross(lVec, rVec).normalized;
            var up = -((rVec + lVec) / 2f).normalized;

            return (forward, up);
        }
    }
}