using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GlobalConstants
{
    #region Explodable Consts
    /// <summary>
    /// The magnitude of the distance the explosive translation should stop at.
    /// </summary>
    public static readonly float EXPLOSION_STOP_DISTANCE = .5f;
    /// <summary>
    /// The speed of which the explodables translate during explosion and collapse
    /// </summary>
    public static readonly float TRANSLATION_SPEED = 2f;
    /// <summary>
    /// The distance cap that we use to reign in the models so their bounds dont freak out. AKA Magic.
    /// </summary>
    public static readonly float CLAMPING_DISTANCE = 1000f;

    #endregion Explodable Consts

    #region NetworkInteractable Consts
    /// <summary>
    /// The value which indicates no one owns the network interactable
    /// </summary>
    public static readonly ulong OWNERSHIP_UNOWNED = ulong.MaxValue;
    /// <summary>
    /// The value which indicates that the object cannot be owned currently
    /// </summary>
    public static readonly ulong OWNERSHIP_LOCKED = ulong.MaxValue - 1;

    #endregion NetworkInteractable Consts

    #region LockConsts
    /// <summary>
    /// The duration that a lock should be active for in seconds
    /// </summary>
    public static readonly int LOCK_DURATION = 3;
    /// <summary>
    /// The maximum amound of time (in seconds) that explodable manager should wait for a lock to be engaged.
    /// </summary>
    public static readonly float MAX_WAIT_TIME = 5f;

    #endregion LockConsts

    #region Model Consts
    public static readonly string[] MODEL_FORMATS = { ".fbx",".blend" };
    #endregion

    #region Annotation Consts
    public static readonly string VOICE_ANNOTATION = "Voice";
    public static readonly string TEXT_ANNOTATION = "Text";
    #endregion Annotation Consts

    #region Utility Consts
    public static readonly string TIME_FORMAT = "HH:mm:ss dd/MM/yyyy";

    /// <summary>
    /// The max recording length in seconds;
    /// </summary>
    public static readonly int RECORDING_MAX_DURATION = 60;
    #endregion Utility Consts

    #region Resource Paths
    public static readonly string MODEL_DIR = "Assets/Resources/Models"; //these need to be relative paths

    public static readonly string PREFAB_DIR = "Assets/Resources/Prefabs";//these need to be relative paths

    /// <summary>
    /// The dir for storing Metadata. This needs to be on persistant data path as the we create the metadata files
    /// at runtime and all build files and directories are read-only. 
    /// Persistant data path eg C:\Users\USERNAME\AppData\LocalLow\DutchPizza\HLSP\Metadata
    /// </summary>
    public static string METADATA_DIR = $"{Application.persistentDataPath}/Metadata";//"Assets/Resources/Metadata";
    /// <summary>
    /// The Dir for storing Annotation data. This needs to be on persistant data path as annotations are
    /// read/writable and directories in the build dir are read only. 
    /// Persistant data path eg C:\Users\USERNAME\AppData\LocalLow\DutchPizza\HLSP\Metadata
    /// </summary>
    public static string ANNOTATION_DIR = $"{Application.persistentDataPath}/AnnotationData";
    #endregion

} 