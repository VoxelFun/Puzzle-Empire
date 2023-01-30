using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialTextureAssigner : MonoBehaviour {

    [Header("Requirement")]
    public Material[] materials;
    public Texture2D[] textures;

	// Use this for initialization
	void Start () {
        Dictionary<string, Texture2D> dictionary = new Dictionary<string, Texture2D>();
        foreach (var texture in textures) {
            dictionary.Add(texture.name, texture);
        }
        foreach (Material material in materials) {
            Debug.Log(material.name.Replace(" ", ""));
            material.mainTexture = dictionary[material.name.Replace(" ", "")];
        }
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
