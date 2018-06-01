using ModCommon;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Xml.Serialization;
using UnityEngine;

namespace redwing
{
    [XmlRoot("ResourceElement")]
    public class ResourceElement
    {
        [XmlElement("scene")]
        public string scene;

        [XmlElement("resource")]
        public string resource;
    }

    [XmlRoot("ResourceScene")]
    public class ResourceScene
    {
        [XmlElement("sceneName")]
        public string sceneName;

        [XmlArray("resourceObjects")]
        public List<string> resourceObjects = new List<string>();
    }

    [XmlRoot("ResourceManagerData")]
    public class ResourceManagerData
    {
        [XmlArray("resourceScenes")]
        public List<ResourceScene> resourceScenes = new List<ResourceScene>();
    }


    public class ResourceManager : GameSingleton<ResourceManager>
    {
        public ResourceManagerData resourceData;

        Dictionary<string, UnityEngine.Object> resources;

        public static void AddResource(ResourceElement data)
        {
            AddResource(data.scene, data.resource);
        }

        public static void AddResource(string scene, string name)
        {
            foreach (var r in Instance.resourceData.resourceScenes)
            {
                if (r.sceneName == scene)
                {
                    if (r.resourceObjects.Contains(name))
                        return;

                    r.resourceObjects.Add(name);
                    return;
                }
            }

            {
                ResourceScene r = new ResourceScene();
                r.sceneName = scene;
                r.resourceObjects.Add(name);
                Instance.resourceData.resourceScenes.Add(r);
            }

            XMLUtils.WriteDataToFile(Application.dataPath + "/resourceManager.xml", Instance.resourceData);
        }

        protected virtual void Awake()
        {
            XMLUtils.ReadDataFromFile(Application.dataPath + "/resourceManager.xml", out resourceData);

            if (resourceData == null)
                resourceData = new ResourceManagerData();

            Preload(resourceData.resourceScenes);
        }

        public static T GetResource<T>(string scene, string name)
            where T : UnityEngine.Object
        {
            if (!Instance.resources.ContainsKey(scene + name))
                return null;

            return (T)Instance.resources[scene + name];
        }

        void Preload(List<ResourceScene> scenes)
        {
            Instance.StartCoroutine(ResourceManager.Instance.DoPreload(scenes));
        }

        IEnumerator DoPreload(List<ResourceScene> scenes)
        {
            resources = new Dictionary<string, UnityEngine.Object>();
            foreach (var rs in scenes)
            {
                bool skipScene = true;

                foreach (string s in rs.resourceObjects)
                {
                    if (resources.ContainsKey(rs.sceneName + s))
                        continue;

                    skipScene = false;
                    break;
                }

                if (skipScene)
                    continue;

                bool keepLoaded = transform;
                if (!UnityEngine.SceneManagement.SceneManager.GetSceneByName(rs.sceneName).isLoaded)
                {
                    keepLoaded = false;
                    UnityEngine.SceneManagement.SceneManager.LoadScene(rs.sceneName);
                    yield return new WaitForEndOfFrame();
                    yield return new WaitForEndOfFrame();
                }

                foreach (string s in rs.resourceObjects)
                {
                    if (resources.ContainsKey(rs.sceneName + s))
                        continue;

                    foreach (var r in Resources.FindObjectsOfTypeAll<UnityEngine.Object>())
                    {
                        if (r.name == s)
                        {
                            resources.Add(rs.sceneName + s, r);
                            break;
                        }
                    }
                }

                if (!keepLoaded)
                {
                    UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(rs.sceneName);
                    yield return new WaitForEndOfFrame();
                    yield return new WaitForEndOfFrame();
                }
            }
        }
    }

    [XmlRoot("GameObjectData")]
    public class GameObjectData
    {
        public static void Save(GameObject go, string path)
        {
            GameObjectData data = new GameObjectData();
            data.Save(path, go);
        }

        public void Save(string path, GameObject go)
        {
            RootPath = path.Substring(0, path.Length - Path.GetExtension(path).Length);
            Serialize(go);
            WriteDataToFile(path, this);
        }

        void Serialize(GameObject go)
        {
            name = go.name;
            tag = go.tag;
            layer = go.layer;
            activeSelf = go.activeSelf;

            componentData = new List<ComponentData>();
            Component[] components = go.GetComponents<Component>();
            for (int i = 0; i < components.Length; ++i)
            {
                ComponentData data = Create(components[i]);
                if (data != null)
                {
                    componentData.Add(data);
                }
                else
                    Debug.Log("Failed to serialize component " + components[i].GetType().Name);
            }

            childrenData = new List<GameObjectData>();
            for (int i = 0; i < go.transform.childCount; ++i)
            {
                GameObjectData childData = new GameObjectData();
                GameObject child = go.transform.GetChild(i).gameObject;
                childData.Serialize(child);
                childrenData.Add(childData);
            }
        }

        ComponentData Create(Component c)
        {
            Type componentType = c.GetType();
            string componentDataTypeName = "SL." + componentType.Name + "Data";

            if (Assembly.GetAssembly(typeof(ComponentData)).GetType(componentDataTypeName) == null)
            {
                //TODO: post warning that the data type is not defined
                Debug.LogWarning(componentDataTypeName + " type not found in " + Assembly.GetAssembly(typeof(ComponentData)) + "; Skipping Serialization.");

                foreach (var t in Assembly.GetAssembly(typeof(ComponentData)).GetTypes())
                {
                    if (t.Name.Contains(componentDataTypeName))
                    {
                        Debug.LogWarning("Did find similar type with name " + t.FullName);
                    }
                }

                return null;
            }

            var handle = Activator.CreateInstance(null, componentDataTypeName);
            ComponentData data = (ComponentData)handle.Unwrap();
            data.assemblyName = componentType.Assembly.FullName;
            data.typeName = componentType.FullName;
            data.owner = this;
            data.Serialize(c);
            return data;
        }

        static public GameObjectData LoadData(string path)
        {
            GameObjectData go;
            ReadDataFromFile(path, out go);
            go.RootPath = path.Substring(0, path.Length - Path.GetExtension(path).Length);
            return go;
        }

        static public GameObject LoadGameObject(string path)
        {
            GameObjectData go;
            ReadDataFromFile(path, out go);
            go.RootPath = path.Substring(0, path.Length - Path.GetExtension(path).Length);
            return go.Deserialize();
        }

        public GameObject Deserialize(Transform parent = null)
        {
            var output = new GameObject(name);
            output.SetActive(false);
            output.tag = tag;
            output.layer = layer;

            output.transform.parent = parent;

            for (int i = 0; i < componentData.Count; ++i)
            {
                Assembly componentAssembly = Assembly.Load(componentData[i].assemblyName);

                if (componentAssembly == null)
                {
                    Debug.LogError("Failed to load assembly " + componentData[i].assemblyName);
                    continue;
                }

                Type componentType = componentAssembly.GetType(componentData[i].typeName);

                if (componentType == null)
                {
                    Debug.LogWarning(componentData[i].typeName + " type not found in " + componentAssembly + "; Skipping Deserialization.");

                    foreach (var t in componentAssembly.GetTypes())
                    {
                        if (t.Name.Contains(componentData[i].typeName))
                        {
                            Debug.LogWarning("Did find similar type with name " + t.FullName);
                        }
                    }
                    continue;
                }

                Component newComponent = null;

                if (componentType.FullName == "UnityEngine.Transform")
                {
                    newComponent = output.transform;
                }
                else
                {
                    newComponent = output.AddComponent(componentType);
                }

                if (newComponent != null)
                {
                    componentData[i].owner = this;
                    componentData[i].Deserialize(newComponent);
                }
            }

            for (int i = 0; i < childrenData.Count; ++i)
            {
                GameObject child = childrenData[i].Deserialize(output.transform);
            }

            output.SetActive(activeSelf);
            return output;
        }

        [XmlIgnore]
        public string RootPath { get; private set; }

        [XmlElement("activeSelf")]
        public bool activeSelf;

        [XmlElement("name")]
        public string name;

        [XmlElement("tag")]
        public string tag;

        [XmlElement("layer")]
        public int layer;

        [XmlArray("componentData")]
        public List<ComponentData> componentData;

        [XmlArray("childrenData")]
        public List<GameObjectData> childrenData;

        static bool WriteDataToFile<T>(string path, T settings) where T : class
        {
            //get the derived types and add them to the xml serializer
            var knownTypes = Assembly.GetAssembly(typeof(ComponentData)).GetTypes().Where(
            t => typeof(ComponentData).IsAssignableFrom(t)).ToArray();

            bool result = false;
            XmlSerializer serializer = new XmlSerializer(typeof(T), knownTypes);
            FileStream fstream = null;
            try
            {
                fstream = new FileStream(path, FileMode.Create);
                serializer.Serialize(fstream, settings);
                result = true;
            }
            catch (System.Exception e)
            {
                Debug.Log("Error creating/saving file " + e.Message);
                //System.Windows.Forms.MessageBox.Show("Error creating/saving file "+ e.Message);
            }
            finally
            {
                fstream.Close();
            }
            return result;
        }

        static bool ReadDataFromFile<T>(string path, out T settings) where T : class
        {
            settings = null;

            if (!File.Exists(path))
            {
                //System.Windows.Forms.MessageBox.Show("No file found at " + path );
                return false;
            }

            //get the derived types and add them to the xml serializer
            var knownTypes = Assembly.GetAssembly(typeof(ComponentData)).GetTypes().Where(
            t => typeof(ComponentData).IsAssignableFrom(t)).ToArray();

            bool returnResult = true;

            XmlSerializer serializer = new XmlSerializer(typeof(T), knownTypes);
            FileStream fstream = null;
            try
            {
                fstream = new FileStream(path, FileMode.Open);
                settings = serializer.Deserialize(fstream) as T;
            }
            catch (System.Exception e)
            {
                Debug.Log("Error loading file " + e.Message);
                //System.Windows.Forms.MessageBox.Show("Error loading file " + e.Message);
                returnResult = false;
            }
            finally
            {
                fstream.Close();
            }

            return returnResult;
        }
    }

    [XmlRoot("ComponentData")]
    public class ComponentData
    {
        public virtual void Serialize(Component c)
        {
            Behaviour mb = c as Behaviour;
            if (mb != null)
            {
                enabled = mb.enabled;
            }
            else
            {
                enabled = true;
            }
        }

        public virtual void Deserialize(Component c)
        {
            Behaviour mb = c as Behaviour;
            if (mb != null)
            {
                mb.enabled = enabled;
            }
        }

        [XmlIgnore]
        public GameObjectData owner;

        [XmlElement("assemblyName")]
        public string assemblyName;

        [XmlElement("typeName")]
        public string typeName;

        [XmlElement("enabled")]
        public bool enabled;
    }

    [XmlRoot("AudioSourceData")]
    public class RedAudioSerial : ComponentData
    {
        string ClipPath
        {
            get
            {
                return "/tmp/grimm/" + clip + ".dat";
            }
        }

        public void SaveClipData(AudioClip c)
        {
            if (File.Exists(ClipPath))
                return;
            
            float[] clipData;
            clipData = new float[c.samples * c.channels];
            c.GetData(clipData, 0);

            foreach (var clipval in clipData)
            {
                Modding.Logger.Log(clipval);
            }
            
            FileStream fstream = null;
            try
            {
                
            }
            catch (System.Exception e)
            {
                Modding.Logger.Log("Error creating/saving file " + e.Message);
            }
            finally
            {
                fstream.Close();
            }
        }

        public float[] LoadClipData()
        {
            if (!File.Exists(ClipPath))
                return null;

            if (cachedClipData != null)
                return cachedClipData;

            float[] clipData = null;

            BinaryFormatter serializer = new BinaryFormatter();
            FileStream fstream = null;
            try
            {
                fstream = new FileStream(ClipPath, FileMode.Open);
                clipData = serializer.Deserialize(fstream) as float[];
            }
            catch (System.Exception e)
            {
                Debug.Log("Error loading file " + e.Message);
            }
            finally
            {
                fstream.Close();
            }

            cachedClipData = clipData;

            return clipData;
        }

        public override void Serialize(Component c)
        {
            base.Serialize(c);
            var component = c as AudioSource;
            pitch = component.pitch;
            volume = component.volume;
            if (component.clip != null)
            {
                clip = component.clip.name;
                channels = component.clip.channels;
                frequency = component.clip.frequency;
                samples = component.clip.samples;
                SaveClipData(component.clip);
            }

            if (component.outputAudioMixerGroup != null)
            {
                audioMixer = new ResourceElement();
                audioMixer.scene = component.gameObject.scene.name;
                audioMixer.resource = component.outputAudioMixerGroup.audioMixer.name;
                ResourceManager.AddResource(audioMixer);

                outputAudioMixerGroup = component.outputAudioMixerGroup.name;
            }
        }

        public override void Deserialize(Component c)
        {
            base.Deserialize(c);
            var component = c as AudioSource;
            component.pitch = pitch;
            component.volume = volume;
            if (!string.IsNullOrEmpty(clip))
            {
                component.clip = AudioClip.Create(clip, samples, channels, frequency, false);
                component.clip.SetData(LoadClipData(), 0);
            }

            if (!string.IsNullOrEmpty(outputAudioMixerGroup))
            {
                UnityEngine.Audio.AudioMixer mixer =
                    ResourceManager.GetResource<UnityEngine.Audio.AudioMixer>(audioMixer.scene, audioMixer.resource);

                if (mixer == null)
                {
                    Debug.LogWarning("Could not find audio mixer in resource manager with name " + audioMixer.resource + " in scene " + audioMixer.scene);
                    return;
                }

                component.outputAudioMixerGroup = mixer.FindMatchingGroups(outputAudioMixerGroup)[0];
            }
        }

        [XmlElement("pitch")]
        public float pitch;

        [XmlElement("volume")]
        public float volume;

        [XmlElement("channels")]
        public int channels;

        [XmlElement("frequency")]
        public int frequency;

        [XmlElement("samples")]
        public int samples;

        [XmlElement("clip")]
        public string clip;

        [XmlIgnore]
        float[] cachedClipData;

        [XmlElement("audioMixer")]
        public ResourceElement audioMixer;

        [XmlElement("outputAudioMixerGroup")]
        public string outputAudioMixerGroup;
    }
}
