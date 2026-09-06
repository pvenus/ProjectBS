import Foundation
import AVFoundation

let url=URL(fileURLWithPath:CommandLine.arguments[1])
let asset=AVURLAsset(url:url)
let group=DispatchGroup(); group.enter()
Task {
  do {
    let duration=try await asset.load(.duration)
    let tracks=try await asset.loadTracks(withMediaType:.video)
    let audio=try await asset.loadTracks(withMediaType:.audio)
    if let t=tracks.first {
      let size=try await t.load(.naturalSize); let rate=try await t.load(.nominalFrameRate); let desc=try await t.load(.formatDescriptions)
      let codec=desc.first.map { CMFormatDescriptionGetMediaSubType($0) } ?? 0
      let chars:[UInt8]=[UInt8((codec>>24)&255),UInt8((codec>>16)&255),UInt8((codec>>8)&255),UInt8(codec&255)]
      let fourcc=String(bytes:chars,encoding:.ascii) ?? "unknown"
      print("duration=\(CMTimeGetSeconds(duration)) width=\(Int(size.width)) height=\(Int(size.height)) fps=\(rate) codec_fourcc=\(fourcc) audio_tracks=\(audio.count)")
    }
  } catch { print("error=\(error)") }
  group.leave()
}
group.wait()
