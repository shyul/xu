/******************************************************************************
*
* niVB_MSO_Example_utf16.c
*
* Description:
*    This examples demonstrates how to configure and acquire data from the
*    Mixed Signal Oscilloscope (MSO) instrument on a VirtualBench.
*
* Instructions for Running:
*    1. Modify the "deviceName" parameter to be the name of the device that is
*       being used.  By default it is set to "virtualbench".
*    2. Modify the acquisition configuration.  These are the defaults:
*          Analog Channel Configuration
*          ============================
*          Channels: mso/1, mso/2
*          Vertical Range: 10V
*          Vertical Offset: 0V
*          Probe Attenutation: 1x
*          Coupling: DC
*
*          Digital Channel Configuration
*          =============================
*          Channels: mso/d0 through mso/31, mso/clk0, and mso/clk1 (34 total)
*
*          Timing Configuration
*          ====================
*          Analog Sample Rate: 500MHz
*          Analog Sampling Mode: Sample
*          Digital Sample Rate: 1GHz
*          Acquisition Time: 12us
*          Pretrigger Time: 6us
*
*          Trigger Configuration
*          =====================
*          Type: Analog Edge Trigger
*          Source: mso/1
*          Level: 1V
*          Hysteresis: Minimum
*          Edge: Rising
*    3. Build and run the program.
*
* Notes:
*    This example utilizes the UTF-16 VirtualBench APIs that are available on
*    Windows.  This example uses wprintf(), which expects UTF-16.  However, the
*    console that is being used may not be able to render Unicode characters
*    correctly.  In particular the default raster font of the Windows console
*    won't render most code points.  Furthermore, the console itself can't
*    handle any code points that require more than one 16-bit (wchar_t) code
*    unit.
******************************************************************************/

#if !defined(_WIN32)
#error This example requires the Windows API.
#endif

#if defined(_CVI_)
#error This example is not supported in CVI.
#endif

#define UNICODE
#include <fcntl.h>
#include <io.h>
#include <stdio.h>
#include <stdlib.h>
#include <nivirtualbench/nivirtualbench.h>
#include <windows.h>

static const wchar_t* getTriggerReasonStr(niVB_MSO_TriggerReason triggerReason)
{
   switch (triggerReason)
   {
      case niVB_MSO_TriggerReason_Auto:   return L"Auto";
      case niVB_MSO_TriggerReason_Forced: return L"Forced";
      case niVB_MSO_TriggerReason_Normal: return L"Normal";
      default: return L"Unknown";
   }
}

static void printTriggerTimestamp(niVB_Timestamp ts)
{
   int64_t seconds;
   double subSeconds;

   /* First, convert the VirtualBench specific timestamp to raw data. */
   if (niVB_Status_Failed(niVB_ConvertTimestampToValues(ts, &seconds, &subSeconds)))
   {
      wprintf(L"Error converting timestamp to values.\n");
      return;
   }

   /* Now convert to a user-visible time either using Windows or C functions. */
   {
      FILETIME utcFileTime;
      FILETIME localFileTime;
      LONGLONG rawSystemTimeInNs;
      SYSTEMTIME systemTime;
      wchar_t *timeString;
      int timeStringSize;

      rawSystemTimeInNs = seconds * 10000000 /* nanoseconds per second */ + 116444736000000000 /* ns between Windows and Unix epochs */;
      utcFileTime.dwLowDateTime = (DWORD)rawSystemTimeInNs;
      utcFileTime.dwHighDateTime = (DWORD)(rawSystemTimeInNs >> 32);
      if (FileTimeToLocalFileTime(&utcFileTime, &localFileTime) == 0 ||
         FileTimeToSystemTime(&localFileTime, &systemTime) == 0)
      {
         wprintf(L"Error converting filetime to systemtime.\n");
         return;
      }

      timeStringSize = GetTimeFormatW(LOCALE_USER_DEFAULT, 0, &systemTime, NULL, NULL, 0);
      timeString = (wchar_t*)malloc(timeStringSize * sizeof(wchar_t));
      if (GetTimeFormatW(LOCALE_USER_DEFAULT, 0, &systemTime, NULL, timeString, timeStringSize) != 0)
      {
         wprintf(L"Trigger Timestamp: %s\n\t%g fractional seconds\n", timeString, subSeconds);
      }
      else
      {
         wprintf(L"Failed to convert systemtime to string.\n");
      }

      free(timeString);
   }
}

static double getTimestampDifference(niVB_Timestamp a, niVB_Timestamp b)
{
   int64_t aSeconds, bSeconds;
   double aSubSeconds, bSubSeconds;

   if (niVB_Status_Failed(niVB_ConvertTimestampToValues(a, &aSeconds, &aSubSeconds)) ||
       niVB_Status_Failed(niVB_ConvertTimestampToValues(b, &bSeconds, &bSubSeconds)))
   {
      return 0.0;
   }

   return (double)(aSeconds - bSeconds) + aSubSeconds - bSubSeconds;
}

static void printAnalogData(double *data, size_t size, size_t stride, size_t numChannels, niVB_MSO_SamplingMode sampleMode, size_t numToPrint)
{
   size_t samp, chan, numSamplesAcquired;

   numSamplesAcquired = size / stride;
   if (numSamplesAcquired < numToPrint) numToPrint = numSamplesAcquired;

   wprintf(L"Analog Data (%d of %d):\n", numToPrint, numSamplesAcquired);

   /* Print channel header information. */
   wprintf(L"Channel:\t");
   for (chan = 0; chan < numChannels; ++chan)
   {
      if (chan != 0) wprintf(L"\t\t");
      wprintf(L"%d", chan);
      if (sampleMode == niVB_MSO_SamplingMode_PeakDetect)
      {
         wprintf(L" (min)\t\t%d (max)", chan);
      }
   }
   wprintf(L"\n");

   /* Print the data itself. */
   for (samp = 0; samp < numToPrint; ++samp)
   {
      wprintf(L"Sample %d:\t", samp);
      for (chan = 0; chan < stride; ++chan)
      {
         if (chan != 0) wprintf(L"\t");
         wprintf(L"%4.6f", data[samp * stride + chan]);
      }
      wprintf(L"\n");
   }
}

static void printDigitalData(uint64_t *data, uint32_t *timestamps, size_t size, size_t numToPrint)
{
   size_t samp;

   if (size < numToPrint) numToPrint = size;

   wprintf(L"Digital Data (%d of %d):\n", numToPrint, size);
   wprintf(L"\t\tTimestamp\tData\n");
   for (samp = 0; samp < numToPrint; ++samp)
   {
      wprintf(L"Sample %d:\t%d\t\t0x%09llX\n", samp, timestamps[samp], data[samp]);
   }
}

#define niVB_ErrorCheck(function) if (niVB_Status_Assign(&status, (function)) && niVB_Status_Failed(status)) { goto Cleanup; }

int __cdecl main(void)
{
   /* Global Configuration */
   niVB_Status status = niVB_Status_Success;
   niVB_LibraryHandle libHandle = NULL;
   const wchar_t *deviceName = L"virtualbench";
   niVB_MSO_InstrumentHandle msoHandle = NULL;

   /* Analog Channel Configuration */
   const wchar_t *analogChannels = L"mso/1:2";
   const size_t numAnalogChannels = 2;
   const double verticalRange = 10.0;
   const double verticalOffset = 0.0;
   const niVB_MSO_ProbeAttenuation probeAttenuation = niVB_MSO_ProbeAttenuation_1x;
   const niVB_MSO_Coupling coupling = niVB_MSO_Coupling_DC;

   /* Digital Channel Configuration */
   const wchar_t *digitalChannels = L"mso/d0:31, mso/clk0:1";

   /* Timing Configuration */
   const double analogSampleRate = 500e6;
   const double digitalSampleRate = 1e9;
   const double acquisitionTime = 12e-6;
   const double pretriggerTime = 6e-6;
   const niVB_MSO_SamplingMode samplingMode = niVB_MSO_SamplingMode_Sample;

   /* Trigger Configuration */
   const wchar_t *triggerSource = L"mso/1";
   const niVB_EdgeWithEither triggerSlope = niVB_EdgeWithEither_Rising;
   const double triggerLevel = 1.0;
   const double triggerHysteresis = 0.0; /* Minimum */

   /* Data */
   size_t analogDataSize = 0, analogDataStride = 0, digitalDataSize = 0, digitalTimestampsSize = 0;
   double *analogData = NULL;
   uint64_t *digitalData = NULL;
   uint32_t *digitalTimestamps = NULL;
   niVB_Timestamp analogT0, digitalT0, triggerTimestamp;
   niVB_MSO_TriggerReason triggerReason;
   size_t numSamplesToPrint = 10;

   #if defined(_MSC_VER) && _MSC_VER >= 1400
   /* Enable UTF-16 on the console.  This is only available in Visual Studio 2005 and later. */
   _setmode(_fileno(stdout), _O_U16TEXT);
   #endif

   niVB_ErrorCheck(niVB_Initialize(NIVB_LIBRARY_VERSION, &libHandle));

   /* Initialize the instrument, this can fail if the device is unreachable or the instrument is reserved. */
   niVB_ErrorCheck(niVB_MSO_InitializeW(libHandle, deviceName, /* reset: */ true, &msoHandle));

   /* Configure the analog and digital channels. */
   niVB_ErrorCheck(niVB_MSO_ConfigureAnalogChannelW(msoHandle, analogChannels, /* enable: */ true, verticalRange, verticalOffset, probeAttenuation, coupling));
   niVB_ErrorCheck(niVB_MSO_EnableDigitalChannelsW(msoHandle, digitalChannels, /* enable: */ true));

   /* Configure timing. */
   niVB_ErrorCheck(niVB_MSO_ConfigureTiming(msoHandle, analogSampleRate, acquisitionTime, pretriggerTime, samplingMode));
   niVB_ErrorCheck(niVB_MSO_ConfigureAdvancedDigitalTiming(msoHandle, /* rate control: */ niVB_MSO_DigitalSampleRateControl_Manual, digitalSampleRate, /* buffer control: */ niVB_MSO_BufferControl_Automatic, 0.0));

   /* Configure trigger condition. */
   niVB_ErrorCheck(niVB_MSO_ConfigureAnalogEdgeTriggerW(msoHandle, triggerSource, triggerSlope, triggerLevel, triggerHysteresis, niVB_MSO_TriggerInstance_A));

   /* Start the acquisition.  Auto triggering is enabled to catch a misconfigured trigger condition. */
   niVB_ErrorCheck(niVB_MSO_Run(msoHandle, /* autoTrigger: */ true));

   /* Read the data by first querying how big the data needs to be, allocating the memory, and finally performing the read. */
   niVB_ErrorCheck(niVB_MSO_ReadAnalogDigitalU64(
      msoHandle,
      NULL,
      0,
      &analogDataSize,
      NULL,
      NULL,
      NULL,
      0,
      &digitalDataSize,
      NULL,
      0,
      &digitalTimestampsSize,
      NULL,
      NULL,
      NULL));

   analogData = (double*)malloc(analogDataSize*sizeof(double));
   digitalData = (uint64_t*)malloc(digitalDataSize*sizeof(uint64_t));
   digitalTimestamps = (uint32_t*)malloc(digitalTimestampsSize*sizeof(uint32_t));

   niVB_ErrorCheck(niVB_MSO_ReadAnalogDigitalU64(
      msoHandle,
      analogData,
      analogDataSize,
      NULL,
      &analogDataStride,
      &analogT0,
      digitalData,
      digitalDataSize,
      NULL,
      digitalTimestamps,
      digitalTimestampsSize,
      NULL,
      &digitalT0,
      &triggerTimestamp,
      &triggerReason));

   /* Finally, print some information about the data. */
   wprintf(L"Trigger Reason: %ls\n", getTriggerReasonStr(triggerReason));
   printTriggerTimestamp(triggerTimestamp);
   wprintf(L"Analog t0 is %g seconds before trigger\n", getTimestampDifference(triggerTimestamp, analogT0));
   wprintf(L"Digital t0 is %g seconds before trigger\n", getTimestampDifference(triggerTimestamp, digitalT0));
   printAnalogData(analogData, analogDataSize, analogDataStride, numAnalogChannels, samplingMode, numSamplesToPrint);
   printDigitalData(digitalData, digitalTimestamps, digitalDataSize, numSamplesToPrint);

Cleanup:
   /* If an error or warning occurred print out any information that is available. */
   if (status != niVB_Status_Success)
   {
      if (libHandle)
      {
         size_t descrSize = 0, extendedErrSize = 0;
         wchar_t *descrBuf = NULL, *extendedErrBuf = NULL;

         niVB_GetErrorDescriptionW(libHandle, status, niVB_Language_CurrentThreadLocale, NULL, 0, &descrSize);
         descrBuf = (wchar_t*)malloc(descrSize*sizeof(wchar_t));
         niVB_GetErrorDescriptionW(libHandle, status, niVB_Language_CurrentThreadLocale, descrBuf, descrSize, NULL);

         niVB_GetExtendedErrorInformationW(libHandle, niVB_Language_CurrentThreadLocale, NULL, 0, &extendedErrSize);
         extendedErrBuf = (wchar_t*)malloc(extendedErrSize*sizeof(wchar_t));
         niVB_GetExtendedErrorInformationW(libHandle, niVB_Language_CurrentThreadLocale, extendedErrBuf, extendedErrSize, NULL);

         wprintf(L"Error/Warning %d occurred\n", status);
         if (descrSize != 0) wprintf(L"Error Description: %ls\n", descrBuf);
         if (extendedErrSize != 0) wprintf(L"Extended Error Information: %ls\n", extendedErrBuf);

         free(descrBuf);
         free(extendedErrBuf);
      }
      else
      {
         wprintf(L"Error/Warning %d occurred before library initialization succeeded.\n", status);
      }
   }

   /* Clean up any memory that was allocated. */
   free(digitalTimestamps);
   free(digitalData);
   free(analogData);

   /* Clean up the handles that were initialized. */
   niVB_MSO_Close(msoHandle);
   niVB_Finalize(libHandle);
   return 0;
}
