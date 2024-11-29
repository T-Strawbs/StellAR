using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// Please do not Remove
/// Orignal Authors:
///     • Marcello Morena - UniSa - morma016@mymail.unisa.edu.au - https://github.com/Morma016
///     • Travis Strawbridge - Unisa - strtk001@mymail.unisa.edu.au - https://github.com/STRTK001

/// Additional Authors:
/// 

/// <summary>
/// Global class for constants that are used by many systems and classes. Essentially its the config file
/// of the application
/// </summary>
public static class GlobalConstants
{
    //Configurations for explodable objects
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

    //Configurations for networked interactables
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
    
    //Configurations for the lock system
    #region LockConsts
    /// <summary>
    /// The duration that a lock should be active for in seconds
    /// </summary>
    public static readonly int LOCK_DURATION = 3;
    /// <summary>
    /// The maximum amound of time (in seconds) that explodable manager should wait for a lock to be engaged.
    /// </summary>
    public static readonly float MAX_WAIT_TIME = 15f;

    #endregion LockConsts

    //Configurations regarding models
    #region Model Consts
    /// <summary>
    /// A collection of model file formats that are accepted by the system.
    /// You can add more formats provided that you make the system support the
    /// format.
    /// </summary>
    public static readonly string[] MODEL_FORMATS = { ".fbx",".blend" };
    #endregion

    //Configurations for Annotations
    #region Annotation Consts
    /// <summary>
    /// string constant for annotation types of Voice.
    /// </summary>
    public static readonly string VOICE_ANNOTATION = "Voice";
    /// <summary>
    /// string constant for annotation types of Text.
    /// </summary>
    public static readonly string TEXT_ANNOTATION = "Text";
    #endregion Annotation Consts

    //Configurations for generating audio files and clips
    #region AudioClip Consts
    /// <summary>
    /// The sample rate we use to create audio files
    /// </summary>
    public static readonly int SAMPLE_RATE = 44100;
    #endregion

    //Configurations for settings that are universal
    #region Utility Consts
    /// <summary>
    /// The format for displaying date times.
    /// </summary>
    public static readonly string TIME_FORMAT = "HH:mm:ss dd/MM/yyyy";
    /// <summary>
    /// The max recording length in seconds;
    /// </summary>
    public static readonly int RECORDING_MAX_DURATION = 60;
    #endregion Utility Consts

    //Path constants
    #region Resource Paths
    /// <summary>
    /// Target path for the Model Directory for users to place their models in
    /// </summary>
    public static readonly string MODEL_DIR = "Assets/Resources/Models"; //these need to be relative paths
    /// <summary>
    /// Target path for the Prefab Directory. This is the location for the system to dump the prefabs
    /// that are are generated on import.
    /// </summary>
    public static readonly string PREFAB_DIR = "Assets/Resources/Prefabs";//these need to be relative paths

    /// <summary>
    /// The dir for storing Metadata. This needs to be on persistant data path as the we create the metadata files
    /// at runtime and all build files and directories are read-only. 
    /// Persistant data path eg C:\Users\USERNAME\AppData\LocalLow\MorenaBridge\StellAR\Metadata
    /// </summary>
    public static string METADATA_DIR = $"{Application.persistentDataPath}/ProjectFiles/Metadata";//"Assets/ProjectFiles/Metadata";
    /// <summary>
    /// The Dir for storing Annotation data. This needs to be on persistant data path as annotations are
    /// read/writable and directories in the build dir are read only. 
    /// Persistant data path eg C:\Users\USERNAME\AppData\LocalLow\MorenaBridge\StellAR\Metadata
    /// </summary>
    public static string ANNOTATION_DIR = $"{Application.persistentDataPath}/ProjectFiles/AnnotationData";
    #endregion

} 