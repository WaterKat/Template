DIRECTIONS
~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
1.(Recommended but not Mandatory)Place your audio files in "WaterKat/AudioManager/AudioClips/"
2.Go to "WaterKat/AudioManager/AudioInterfaces" and right click and follow  "Create/WaterKat/Audio/AudioInterface"
3.Update all desired settings on the AudioInterface ScriptableObject, especially the AudioMixerGroup and AudioClip fields (Yes even the name)
4.Repeat 2 and 3 for all sound clips
5.Click and drag the AudioManager Prefab from "WaterKat/AudioManager/" to the scene.
6.Update the MainAudioPath if using a different folder for AudioInterface files
6.Update the StartingAudioname if you'd like an audioclip to play on Start()
7.Right click the AudioManager component in the prefab in the active scene and select UpdateAssets()
8.Profit

You should be able to use static methods to play all your sounds now

EXAMPLE				
~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
/*

using UnityEngine;
using WaterKat.Audio;

public class ExampleClass : MonoBehaviour
{
    private void Start()
    {
        AudioManager.PlaySound("WK_Audio");
		//OR
		WaterKat.Audio.AudioManager.PlaySound("WK_Audio");
    }
}

*/


FEATURES
~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

EDITOR ONLY VARIABLES											//These are variables that will change the initial behavior of the AudioManager
~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

StartingAudioName: This is the first audio clip that will be loaded on Start()
mainAudioPath: This is the path in which you shoud create you AudioInterface scriptable objects


The following are pretty straight forward

PUBLIC STATIC METHODS											//You can call these methods from any script as long as you use "using WaterKat.Audio"
~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

PlaySound(string _audioName)									//Plays the sound with name _audioName

PlaySoundWithDelay(string _audioName, float _delay)				//Plays the sound with name _audioName after _delay seconds

PauseSound(string _audioName)									//Pauses the sound with name _audioName

unPauseSound(string _audioName)									//UnPauses the sound with name _audioName

StopSound(string _audioName)									//Stops the sound with name 

PlayAudioClipAtPoint(string audioName, Vector3 vector3Point)	//Plays the sound with name _audioName at Vector3 vector3Point


CONTEXT MENU METHODS											//Right click the AudioManager Monobehavior to open the context menu and access these methods
~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

"UpdateAssets() Warning! This WILL override CURRENT DATA!"		//This will erase all contents in the Monobehaviors AudioInterface list and create a new one from mainAudioPath
