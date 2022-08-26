//
//  UnityInterface.m
//  
///Users/mac/Desktop/TryFFmpeg/Assets/Plugins/NativeGallery/iOS/NativeGallery.mm
//  Created by Mac on 2022/8/22.
//
#import "UnityAppController.h"
#import <ffmpegkit/FFmpegKit.h>
#import <ffmpegkit/FFmpegKitConfig.h>
#import <Foundation/Foundation.h>

@interface UnityInterface :NSObject

@end

@implementation UnityInterface

void* cancel(){
    [FFmpegKit cancel];
}

+(void) SendGameObjectName:(NSString*)gameObjectName CompleteCallMethodName:(NSString*)completeMethodName ProgressCallMethodName:(NSString*)progressMethodName AddCmd:(NSString*)cmd VideoDuration:(NSString*) duration{
   /* FFmpegSession *session = [FFmpegKit execute:cmd];
    ReturnCode *returnCode = [session getReturnCode];
    if ([ReturnCode isSuccess:returnCode]) {
        UnitySendMessage([gameObjectName UTF8String],[methodName UTF8String],"0");
        // SUCCESS
    } else if ([ReturnCode isCancel:returnCode]) {
        UnitySendMessage([gameObjectName UTF8String],[methodName UTF8String],"2");
        // CANCEL
    } else {
        UnitySendMessage([gameObjectName UTF8String],[methodName UTF8String],"1");
        // FAILURE
        NSLog(@"Command failed with state %@ and rc %@.%@", [FFmpegKitConfig sessionStateToString:[session getState]], returnCode, [session getFailStackTrace]);
    }*/
    
    FFmpegSession *session = [FFmpegKit executeAsync:cmd withCompleteCallback:^(FFmpegSession* session){
        SessionState state = [session getState];
        ReturnCode *returnCode = [session getReturnCode];

        // CALLED WHEN SESSION IS EXECUTED
        
        if ([ReturnCode isSuccess:returnCode]) {
            UnitySendMessage([gameObjectName UTF8String],[completeMethodName UTF8String],"0");
            // SUCCESS
        } else if ([ReturnCode isCancel:returnCode]) {
            UnitySendMessage([gameObjectName UTF8String],[completeMethodName UTF8String],"2");
            // CANCEL
        } else {
            UnitySendMessage([gameObjectName UTF8String],[completeMethodName UTF8String],"1");
            // FAILURE
        }
        NSLog(@"FFmpeg process exited with state %@ and rc %@.%@", [FFmpegKitConfig sessionStateToString:state], returnCode, [session getFailStackTrace]);
    } withLogCallback:^(Log *log) {

        NSLog(@"%@", [log getMessage]);
        // CALLED WHEN SESSION PRINTS LOGS

    } withStatisticsCallback:^(Statistics *statistics) {
        if(![duration  isEqual:@""]){
            int timeInMilliseconds = [statistics getTime];
            int totalVideoDuration = [duration intValue];

            int percentage = timeInMilliseconds*100/totalVideoDuration;
            // CALLED WHEN SESSION GENERATES STATISTICS
            NSLog(@"Statistics %d",percentage);
            NSString* progress_str =[NSString stringWithFormat:@"%d",percentage];
            UnitySendMessage([gameObjectName UTF8String],[progressMethodName UTF8String],[progress_str UTF8String]);
        }
    }];
}

@end

extern "C" {
    void FFmpegKit_Execute(char* gameObjectName,char* completeMethodName, char* progressMethodName,char* cmd,char* duration){
        [UnityInterface SendGameObjectName:[NSString stringWithUTF8String:gameObjectName] CompleteCallMethodName:[NSString stringWithUTF8String:completeMethodName] ProgressCallMethodName:[NSString stringWithUTF8String:progressMethodName] AddCmd:[NSString stringWithUTF8String:cmd] VideoDuration:[NSString stringWithUTF8String:duration]];
    }

    void Cancel(){
        cancel();
    }
}
/*void UnitySendMessage(NSString* gameObjectName,NSString* MethodName,NSString* Parameter){
    
}*/
