/******************************************************************
** 文件名:  TreeNode.cs
** 版  权:  (C)
** 创建人:  moshoeu
** 日  期:  2022/02/09
** 描  述:  树数据结构

**************************** 修改记录 ******************************
** 修改人: 
** 日  期: 
** 描  述: 
*******************************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TreeNode<T> 
{
    /// <summary>
    /// 子节点
    /// </summary>
    public List<TreeNode<T>> m_Childs;

    /// <summary>
    /// 节点数据
    /// </summary>
    public T m_Data;

    /// <summary>
    /// 父节点
    /// </summary>
    public TreeNode<T> Parent
    {
        get;
        private set;
    }

    /// <summary>
    /// 是否是根节点
    /// </summary>
    public bool IsRoot
    {
        get
        {
            return Parent == null;
        }
    }

    /// <summary>
    /// 层级
    /// </summary>
    public int Layer
    {
        private set;
        get;
    }

    public TreeNode (T data, TreeNode<T> parent = null)
    {
        m_Data = data;
        m_Childs = new List<TreeNode<T>>();

        SetParent(parent);
    }

    /// <summary>
    /// 设置父节点
    /// </summary>
    /// <param name="parent"></param>
    public void SetParent(TreeNode<T> parent)
    {
        // 如果有父节点 设置该节点为子节点
        if (parent != null)
        {
            // 先在当前父节点中移除自己
            Parent?.m_Childs.Remove(this);

            Parent = parent;
            Parent.m_Childs.Add(this);
            Layer = Parent.Layer + 1;
        }
        else
        {
            Layer = 1;
        }
    }
}


