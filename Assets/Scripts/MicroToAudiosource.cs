using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Audio;

public class MicroToAudiosource : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioMixer audioMixer;
    private string selectedDevice;

    void Start()
    {
        if (Microphone.devices.Length > 0)
        {
            selectedDevice = Microphone.devices[0];
            audioSource.clip = Microphone.Start(selectedDevice, true, 10, 44100);

            while (!(Microphone.GetPosition(selectedDevice) > 0)) { }

            audioSource.Play();

            audioSource.volume = 1.5f; // Appliquer un gain modéré
            audioMixer.SetFloat("Volume", 20f);  // Ajuste le gain global à 10 dB
        }
        else
        {
            UnityEngine.Debug.LogError("Aucun microphone détecté !");
        }
    }

    void OnDisable()
    {
        Microphone.End(selectedDevice);
    }
}
