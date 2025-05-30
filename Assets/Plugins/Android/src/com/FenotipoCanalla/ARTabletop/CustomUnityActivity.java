package com.fcanalla.artabletop;

import android.content.Intent;
import android.os.Bundle;
import com.unity3d.player.UnityPlayer;
import com.unity3d.player.UnityPlayerActivity;

public class CustomUnityActivity extends UnityPlayerActivity {
    @Override
    protected void onNewIntent(Intent intent) {
        super.onNewIntent(intent);
        setIntent(intent); // Esto es clave
        if (intent.getDataString() != null) {
            UnityPlayer.UnitySendMessage("GameSaver", "OnIntentReceived", intent.getDataString());
        }
    }
}