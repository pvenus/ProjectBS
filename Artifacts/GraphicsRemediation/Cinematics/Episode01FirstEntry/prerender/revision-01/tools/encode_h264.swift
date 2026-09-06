import Foundation
import AVFoundation
import AppKit
import CoreVideo

let args=CommandLine.arguments
guard args.count==3 else { fatalError("usage: encode_h264 frames_dir output.mp4") }
let dir=URL(fileURLWithPath:args[1]); let out=URL(fileURLWithPath:args[2])
try? FileManager.default.removeItem(at:out)
let writer=try AVAssetWriter(outputURL:out,fileType:.mp4)
let settings:[String:Any]=[AVVideoCodecKey:AVVideoCodecType.h264,AVVideoWidthKey:960,AVVideoHeightKey:540,AVVideoCompressionPropertiesKey:[AVVideoProfileLevelKey:AVVideoProfileLevelH264HighAutoLevel,AVVideoAverageBitRateKey:5_000_000,AVVideoExpectedSourceFrameRateKey:30,AVVideoMaxKeyFrameIntervalKey:30]]
let input=AVAssetWriterInput(mediaType:.video,outputSettings:settings); input.expectsMediaDataInRealTime=false
let adaptor=AVAssetWriterInputPixelBufferAdaptor(assetWriterInput:input,sourcePixelBufferAttributes:[kCVPixelBufferPixelFormatTypeKey as String:kCVPixelFormatType_32BGRA,kCVPixelBufferWidthKey as String:960,kCVPixelBufferHeightKey as String:540])
guard writer.canAdd(input) else { fatalError("cannot add input") }; writer.add(input); writer.startWriting(); writer.startSession(atSourceTime:.zero)
let queue=DispatchQueue(label:"encode")
input.requestMediaDataWhenReady(on:queue) {
  var i=0
  while input.isReadyForMoreMediaData && i<240 {
    autoreleasepool {
      let u=dir.appendingPathComponent(String(format:"frame-%04d.png",i)); guard let img=NSImage(contentsOf:u), let cg=img.cgImage(forProposedRect:nil,context:nil,hints:nil) else { fatalError("missing frame \(i)") }
      var pb:CVPixelBuffer?; CVPixelBufferCreate(kCFAllocatorDefault,960,540,kCVPixelFormatType_32BGRA,[kCVPixelBufferCGImageCompatibilityKey:true,kCVPixelBufferCGBitmapContextCompatibilityKey:true] as CFDictionary,&pb)
      guard let p=pb else { fatalError("pixel buffer") }; CVPixelBufferLockBaseAddress(p,[])
      let ctx=CGContext(data:CVPixelBufferGetBaseAddress(p),width:960,height:540,bitsPerComponent:8,bytesPerRow:CVPixelBufferGetBytesPerRow(p),space:CGColorSpaceCreateDeviceRGB(),bitmapInfo:CGImageAlphaInfo.premultipliedFirst.rawValue|CGBitmapInfo.byteOrder32Little.rawValue)!
      ctx.translateBy(x:0,y:540); ctx.scaleBy(x:1,y:-1); ctx.draw(cg,in:CGRect(x:0,y:0,width:960,height:540)); CVPixelBufferUnlockBaseAddress(p,[])
      if !adaptor.append(p,withPresentationTime:CMTime(value:CMTimeValue(i),timescale:30)) { fatalError("append \(i)") }
      i += 1
    }
  }
  if i==240 { input.markAsFinished(); writer.finishWriting { print("status=\(writer.status.rawValue) error=\(String(describing:writer.error))"); exit(writer.status == .completed ? 0 : 2) } }
}
RunLoop.main.run()
