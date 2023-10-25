using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Lift.Core.Common;

/**
 * Pixel        : 图像元素，图像的最小单位
 * Resolution   : 图像分辨率，图像的宽高
 * W & H & D    : 图像的宽高深度，是图像的物理信息
 */


/// <summary>
/// 图像的元数据
/// </summary>
public class ImageMetadata
{
    /// <summary>
    /// 是否需要承载多个元数据
    /// </summary>
    public bool IsMulti { get; set; } = false;

    /// <summary>
    /// 当前元数据子目录
    /// </summary>
    public Dictionary<string, ImageMetadata>? Metadatas { get; set; }

    ///// <summary>
    ///// Width - X - Cols
    ///// </summary>
    //public ImageAxis? Width { get; set; }

    ///// <summary>
    ///// Height - Y - Rows
    ///// </summary>
    //public ImageAxis? Height { get; set; }

    ///// <summary>
    ///// Depth - Z -Stack
    ///// </summary>
    //public ImageAxis? Depth { get; set; }

    /// <summary>
    /// 图像的像素分辨率
    /// </summary>
    public Vector? Resolution { get; set; }

    /// <summary>
    /// 图像的实际尺寸
    /// </summary>
    public Vector? Size { get; set; }

    /// <summary>
    /// 图像的缩放信息
    /// </summary>
    public Vector? Scale { get; set; }

    /// <summary>
    /// 图像的单位信息
    /// </summary>
    public string? Uint { get; set; }
}
