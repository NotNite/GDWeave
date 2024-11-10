use crate::export_indices::*;
use crate::{ORIGINAL_FUNCS, ORIG_DLL_HANDLE};
use std::ffi::CString;
use winapi::{
    shared::minwindef::{FARPROC, HMODULE},
    um::libloaderapi::GetProcAddress,
};

/// Loads up the address of the original function in the given module
unsafe fn load_dll_func(index: usize, h_module: HMODULE, func: &str) {
    let func_c_string = CString::new(func).unwrap();
    let proc_address: FARPROC = GetProcAddress(h_module, func_c_string.as_ptr());
    ORIGINAL_FUNCS[index] = proc_address;
    println!("[0x{:016x}] Loaded {}", proc_address as u64, func);
}

/// Loads the original DLL functions for later use
pub unsafe fn load_dll_funcs() {
    println!("Loading original DLL functions");
    if ORIG_DLL_HANDLE.is_none() {
        eprintln!("Original DLL handle is none. Cannot load original DLL funcs");
        return;
    }
    let dll_handle = ORIG_DLL_HANDLE.unwrap();
    load_dll_func(Index_CloseDriver, dll_handle, "CloseDriver");
    load_dll_func(Index_DefDriverProc, dll_handle, "DefDriverProc");
    load_dll_func(Index_DriverCallback, dll_handle, "DriverCallback");
    load_dll_func(Index_DrvGetModuleHandle, dll_handle, "DrvGetModuleHandle");
    load_dll_func(
        Index_GetDriverModuleHandle,
        dll_handle,
        "GetDriverModuleHandle",
    );
    load_dll_func(Index_NotifyCallbackData, dll_handle, "NotifyCallbackData");
    load_dll_func(Index_OpenDriver, dll_handle, "OpenDriver");
    load_dll_func(Index_PlaySound, dll_handle, "PlaySound");
    load_dll_func(Index_PlaySoundA, dll_handle, "PlaySoundA");
    load_dll_func(Index_PlaySoundW, dll_handle, "PlaySoundW");
    load_dll_func(Index_SendDriverMessage, dll_handle, "SendDriverMessage");
    load_dll_func(Index_WOW32DriverCallback, dll_handle, "WOW32DriverCallback");
    load_dll_func(Index_WOWAppExit, dll_handle, "WOWAppExit");
    load_dll_func(Index_aux32Message, dll_handle, "aux32Message");
    load_dll_func(Index_auxGetDevCapsA, dll_handle, "auxGetDevCapsA");
    load_dll_func(Index_auxGetDevCapsW, dll_handle, "auxGetDevCapsW");
    load_dll_func(Index_auxGetNumDevs, dll_handle, "auxGetNumDevs");
    load_dll_func(Index_auxGetVolume, dll_handle, "auxGetVolume");
    load_dll_func(Index_auxOutMessage, dll_handle, "auxOutMessage");
    load_dll_func(Index_auxSetVolume, dll_handle, "auxSetVolume");
    load_dll_func(Index_joy32Message, dll_handle, "joy32Message");
    load_dll_func(Index_joyConfigChanged, dll_handle, "joyConfigChanged");
    load_dll_func(Index_joyGetDevCapsA, dll_handle, "joyGetDevCapsA");
    load_dll_func(Index_joyGetDevCapsW, dll_handle, "joyGetDevCapsW");
    load_dll_func(Index_joyGetNumDevs, dll_handle, "joyGetNumDevs");
    load_dll_func(Index_joyGetPos, dll_handle, "joyGetPos");
    load_dll_func(Index_joyGetPosEx, dll_handle, "joyGetPosEx");
    load_dll_func(Index_joyGetThreshold, dll_handle, "joyGetThreshold");
    load_dll_func(Index_joyReleaseCapture, dll_handle, "joyReleaseCapture");
    load_dll_func(Index_joySetCapture, dll_handle, "joySetCapture");
    load_dll_func(Index_joySetThreshold, dll_handle, "joySetThreshold");
    load_dll_func(Index_mci32Message, dll_handle, "mci32Message");
    load_dll_func(Index_mciDriverNotify, dll_handle, "mciDriverNotify");
    load_dll_func(Index_mciDriverYield, dll_handle, "mciDriverYield");
    load_dll_func(Index_mciExecute, dll_handle, "mciExecute");
    load_dll_func(
        Index_mciFreeCommandResource,
        dll_handle,
        "mciFreeCommandResource",
    );
    load_dll_func(Index_mciGetCreatorTask, dll_handle, "mciGetCreatorTask");
    load_dll_func(Index_mciGetDeviceIDA, dll_handle, "mciGetDeviceIDA");
    load_dll_func(
        Index_mciGetDeviceIDFromElementIDA,
        dll_handle,
        "mciGetDeviceIDFromElementIDA",
    );
    load_dll_func(
        Index_mciGetDeviceIDFromElementIDW,
        dll_handle,
        "mciGetDeviceIDFromElementIDW",
    );
    load_dll_func(Index_mciGetDeviceIDW, dll_handle, "mciGetDeviceIDW");
    load_dll_func(Index_mciGetDriverData, dll_handle, "mciGetDriverData");
    load_dll_func(Index_mciGetErrorStringA, dll_handle, "mciGetErrorStringA");
    load_dll_func(Index_mciGetErrorStringW, dll_handle, "mciGetErrorStringW");
    load_dll_func(Index_mciGetYieldProc, dll_handle, "mciGetYieldProc");
    load_dll_func(
        Index_mciLoadCommandResource,
        dll_handle,
        "mciLoadCommandResource",
    );
    load_dll_func(Index_mciSendCommandA, dll_handle, "mciSendCommandA");
    load_dll_func(Index_mciSendCommandW, dll_handle, "mciSendCommandW");
    load_dll_func(Index_mciSendStringA, dll_handle, "mciSendStringA");
    load_dll_func(Index_mciSendStringW, dll_handle, "mciSendStringW");
    load_dll_func(Index_mciSetDriverData, dll_handle, "mciSetDriverData");
    load_dll_func(Index_mciSetYieldProc, dll_handle, "mciSetYieldProc");
    load_dll_func(Index_mid32Message, dll_handle, "mid32Message");
    load_dll_func(Index_midiConnect, dll_handle, "midiConnect");
    load_dll_func(Index_midiDisconnect, dll_handle, "midiDisconnect");
    load_dll_func(Index_midiInAddBuffer, dll_handle, "midiInAddBuffer");
    load_dll_func(Index_midiInClose, dll_handle, "midiInClose");
    load_dll_func(Index_midiInGetDevCapsA, dll_handle, "midiInGetDevCapsA");
    load_dll_func(Index_midiInGetDevCapsW, dll_handle, "midiInGetDevCapsW");
    load_dll_func(Index_midiInGetErrorTextA, dll_handle, "midiInGetErrorTextA");
    load_dll_func(Index_midiInGetErrorTextW, dll_handle, "midiInGetErrorTextW");
    load_dll_func(Index_midiInGetID, dll_handle, "midiInGetID");
    load_dll_func(Index_midiInGetNumDevs, dll_handle, "midiInGetNumDevs");
    load_dll_func(Index_midiInMessage, dll_handle, "midiInMessage");
    load_dll_func(Index_midiInOpen, dll_handle, "midiInOpen");
    load_dll_func(Index_midiInPrepareHeader, dll_handle, "midiInPrepareHeader");
    load_dll_func(Index_midiInReset, dll_handle, "midiInReset");
    load_dll_func(Index_midiInStart, dll_handle, "midiInStart");
    load_dll_func(Index_midiInStop, dll_handle, "midiInStop");
    load_dll_func(
        Index_midiInUnprepareHeader,
        dll_handle,
        "midiInUnprepareHeader",
    );
    load_dll_func(
        Index_midiOutCacheDrumPatches,
        dll_handle,
        "midiOutCacheDrumPatches",
    );
    load_dll_func(Index_midiOutCachePatches, dll_handle, "midiOutCachePatches");
    load_dll_func(Index_midiOutClose, dll_handle, "midiOutClose");
    load_dll_func(Index_midiOutGetDevCapsA, dll_handle, "midiOutGetDevCapsA");
    load_dll_func(Index_midiOutGetDevCapsW, dll_handle, "midiOutGetDevCapsW");
    load_dll_func(
        Index_midiOutGetErrorTextA,
        dll_handle,
        "midiOutGetErrorTextA",
    );
    load_dll_func(
        Index_midiOutGetErrorTextW,
        dll_handle,
        "midiOutGetErrorTextW",
    );
    load_dll_func(Index_midiOutGetID, dll_handle, "midiOutGetID");
    load_dll_func(Index_midiOutGetNumDevs, dll_handle, "midiOutGetNumDevs");
    load_dll_func(Index_midiOutGetVolume, dll_handle, "midiOutGetVolume");
    load_dll_func(Index_midiOutLongMsg, dll_handle, "midiOutLongMsg");
    load_dll_func(Index_midiOutMessage, dll_handle, "midiOutMessage");
    load_dll_func(Index_midiOutOpen, dll_handle, "midiOutOpen");
    load_dll_func(
        Index_midiOutPrepareHeader,
        dll_handle,
        "midiOutPrepareHeader",
    );
    load_dll_func(Index_midiOutReset, dll_handle, "midiOutReset");
    load_dll_func(Index_midiOutSetVolume, dll_handle, "midiOutSetVolume");
    load_dll_func(Index_midiOutShortMsg, dll_handle, "midiOutShortMsg");
    load_dll_func(
        Index_midiOutUnprepareHeader,
        dll_handle,
        "midiOutUnprepareHeader",
    );
    load_dll_func(Index_midiStreamClose, dll_handle, "midiStreamClose");
    load_dll_func(Index_midiStreamOpen, dll_handle, "midiStreamOpen");
    load_dll_func(Index_midiStreamOut, dll_handle, "midiStreamOut");
    load_dll_func(Index_midiStreamPause, dll_handle, "midiStreamPause");
    load_dll_func(Index_midiStreamPosition, dll_handle, "midiStreamPosition");
    load_dll_func(Index_midiStreamProperty, dll_handle, "midiStreamProperty");
    load_dll_func(Index_midiStreamRestart, dll_handle, "midiStreamRestart");
    load_dll_func(Index_midiStreamStop, dll_handle, "midiStreamStop");
    load_dll_func(Index_mixerClose, dll_handle, "mixerClose");
    load_dll_func(
        Index_mixerGetControlDetailsA,
        dll_handle,
        "mixerGetControlDetailsA",
    );
    load_dll_func(
        Index_mixerGetControlDetailsW,
        dll_handle,
        "mixerGetControlDetailsW",
    );
    load_dll_func(Index_mixerGetDevCapsA, dll_handle, "mixerGetDevCapsA");
    load_dll_func(Index_mixerGetDevCapsW, dll_handle, "mixerGetDevCapsW");
    load_dll_func(Index_mixerGetID, dll_handle, "mixerGetID");
    load_dll_func(
        Index_mixerGetLineControlsA,
        dll_handle,
        "mixerGetLineControlsA",
    );
    load_dll_func(
        Index_mixerGetLineControlsW,
        dll_handle,
        "mixerGetLineControlsW",
    );
    load_dll_func(Index_mixerGetLineInfoA, dll_handle, "mixerGetLineInfoA");
    load_dll_func(Index_mixerGetLineInfoW, dll_handle, "mixerGetLineInfoW");
    load_dll_func(Index_mixerGetNumDevs, dll_handle, "mixerGetNumDevs");
    load_dll_func(Index_mixerMessage, dll_handle, "mixerMessage");
    load_dll_func(Index_mixerOpen, dll_handle, "mixerOpen");
    load_dll_func(
        Index_mixerSetControlDetails,
        dll_handle,
        "mixerSetControlDetails",
    );
    load_dll_func(Index_mmDrvInstall, dll_handle, "mmDrvInstall");
    load_dll_func(Index_mmGetCurrentTask, dll_handle, "mmGetCurrentTask");
    load_dll_func(Index_mmTaskBlock, dll_handle, "mmTaskBlock");
    load_dll_func(Index_mmTaskCreate, dll_handle, "mmTaskCreate");
    load_dll_func(Index_mmTaskSignal, dll_handle, "mmTaskSignal");
    load_dll_func(Index_mmTaskYield, dll_handle, "mmTaskYield");
    load_dll_func(Index_mmioAdvance, dll_handle, "mmioAdvance");
    load_dll_func(Index_mmioAscend, dll_handle, "mmioAscend");
    load_dll_func(Index_mmioClose, dll_handle, "mmioClose");
    load_dll_func(Index_mmioCreateChunk, dll_handle, "mmioCreateChunk");
    load_dll_func(Index_mmioDescend, dll_handle, "mmioDescend");
    load_dll_func(Index_mmioFlush, dll_handle, "mmioFlush");
    load_dll_func(Index_mmioGetInfo, dll_handle, "mmioGetInfo");
    load_dll_func(Index_mmioInstallIOProcA, dll_handle, "mmioInstallIOProcA");
    load_dll_func(Index_mmioInstallIOProcW, dll_handle, "mmioInstallIOProcW");
    load_dll_func(Index_mmioOpenA, dll_handle, "mmioOpenA");
    load_dll_func(Index_mmioOpenW, dll_handle, "mmioOpenW");
    load_dll_func(Index_mmioRead, dll_handle, "mmioRead");
    load_dll_func(Index_mmioRenameA, dll_handle, "mmioRenameA");
    load_dll_func(Index_mmioRenameW, dll_handle, "mmioRenameW");
    load_dll_func(Index_mmioSeek, dll_handle, "mmioSeek");
    load_dll_func(Index_mmioSendMessage, dll_handle, "mmioSendMessage");
    load_dll_func(Index_mmioSetBuffer, dll_handle, "mmioSetBuffer");
    load_dll_func(Index_mmioSetInfo, dll_handle, "mmioSetInfo");
    load_dll_func(Index_mmioStringToFOURCCA, dll_handle, "mmioStringToFOURCCA");
    load_dll_func(Index_mmioStringToFOURCCW, dll_handle, "mmioStringToFOURCCW");
    load_dll_func(Index_mmioWrite, dll_handle, "mmioWrite");
    load_dll_func(Index_mmsystemGetVersion, dll_handle, "mmsystemGetVersion");
    load_dll_func(Index_mod32Message, dll_handle, "mod32Message");
    load_dll_func(Index_mxd32Message, dll_handle, "mxd32Message");
    load_dll_func(Index_sndPlaySoundA, dll_handle, "sndPlaySoundA");
    load_dll_func(Index_sndPlaySoundW, dll_handle, "sndPlaySoundW");
    load_dll_func(Index_tid32Message, dll_handle, "tid32Message");
    load_dll_func(Index_timeBeginPeriod, dll_handle, "timeBeginPeriod");
    load_dll_func(Index_timeEndPeriod, dll_handle, "timeEndPeriod");
    load_dll_func(Index_timeGetDevCaps, dll_handle, "timeGetDevCaps");
    load_dll_func(Index_timeGetSystemTime, dll_handle, "timeGetSystemTime");
    load_dll_func(Index_timeGetTime, dll_handle, "timeGetTime");
    load_dll_func(Index_timeKillEvent, dll_handle, "timeKillEvent");
    load_dll_func(Index_timeSetEvent, dll_handle, "timeSetEvent");
    load_dll_func(Index_waveInAddBuffer, dll_handle, "waveInAddBuffer");
    load_dll_func(Index_waveInClose, dll_handle, "waveInClose");
    load_dll_func(Index_waveInGetDevCapsA, dll_handle, "waveInGetDevCapsA");
    load_dll_func(Index_waveInGetDevCapsW, dll_handle, "waveInGetDevCapsW");
    load_dll_func(Index_waveInGetErrorTextA, dll_handle, "waveInGetErrorTextA");
    load_dll_func(Index_waveInGetErrorTextW, dll_handle, "waveInGetErrorTextW");
    load_dll_func(Index_waveInGetID, dll_handle, "waveInGetID");
    load_dll_func(Index_waveInGetNumDevs, dll_handle, "waveInGetNumDevs");
    load_dll_func(Index_waveInGetPosition, dll_handle, "waveInGetPosition");
    load_dll_func(Index_waveInMessage, dll_handle, "waveInMessage");
    load_dll_func(Index_waveInOpen, dll_handle, "waveInOpen");
    load_dll_func(Index_waveInPrepareHeader, dll_handle, "waveInPrepareHeader");
    load_dll_func(Index_waveInReset, dll_handle, "waveInReset");
    load_dll_func(Index_waveInStart, dll_handle, "waveInStart");
    load_dll_func(Index_waveInStop, dll_handle, "waveInStop");
    load_dll_func(
        Index_waveInUnprepareHeader,
        dll_handle,
        "waveInUnprepareHeader",
    );
    load_dll_func(Index_waveOutBreakLoop, dll_handle, "waveOutBreakLoop");
    load_dll_func(Index_waveOutClose, dll_handle, "waveOutClose");
    load_dll_func(Index_waveOutGetDevCapsA, dll_handle, "waveOutGetDevCapsA");
    load_dll_func(Index_waveOutGetDevCapsW, dll_handle, "waveOutGetDevCapsW");
    load_dll_func(
        Index_waveOutGetErrorTextA,
        dll_handle,
        "waveOutGetErrorTextA",
    );
    load_dll_func(
        Index_waveOutGetErrorTextW,
        dll_handle,
        "waveOutGetErrorTextW",
    );
    load_dll_func(Index_waveOutGetID, dll_handle, "waveOutGetID");
    load_dll_func(Index_waveOutGetNumDevs, dll_handle, "waveOutGetNumDevs");
    load_dll_func(Index_waveOutGetPitch, dll_handle, "waveOutGetPitch");
    load_dll_func(
        Index_waveOutGetPlaybackRate,
        dll_handle,
        "waveOutGetPlaybackRate",
    );
    load_dll_func(Index_waveOutGetPosition, dll_handle, "waveOutGetPosition");
    load_dll_func(Index_waveOutGetVolume, dll_handle, "waveOutGetVolume");
    load_dll_func(Index_waveOutMessage, dll_handle, "waveOutMessage");
    load_dll_func(Index_waveOutOpen, dll_handle, "waveOutOpen");
    load_dll_func(Index_waveOutPause, dll_handle, "waveOutPause");
    load_dll_func(
        Index_waveOutPrepareHeader,
        dll_handle,
        "waveOutPrepareHeader",
    );
    load_dll_func(Index_waveOutReset, dll_handle, "waveOutReset");
    load_dll_func(Index_waveOutRestart, dll_handle, "waveOutRestart");
    load_dll_func(Index_waveOutSetPitch, dll_handle, "waveOutSetPitch");
    load_dll_func(
        Index_waveOutSetPlaybackRate,
        dll_handle,
        "waveOutSetPlaybackRate",
    );
    load_dll_func(Index_waveOutSetVolume, dll_handle, "waveOutSetVolume");
    load_dll_func(
        Index_waveOutUnprepareHeader,
        dll_handle,
        "waveOutUnprepareHeader",
    );
    load_dll_func(Index_waveOutWrite, dll_handle, "waveOutWrite");
    load_dll_func(Index_wid32Message, dll_handle, "wid32Message");
    load_dll_func(Index_wod32Message, dll_handle, "wod32Message");
}
