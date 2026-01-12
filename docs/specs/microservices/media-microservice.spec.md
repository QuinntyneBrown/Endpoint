# Media Microservice Specification

## Document Information
- **Version**: 1.0
- **Last Updated**: 2026-01-12
- **Status**: Active

## 1. Overview
Processes images, videos, and audio including compression, thumbnails, and format conversion.

## 2. Microservice Structure

### Core Layer (Media.Core)
- Entities: MediaFile, Thumbnail, TranscodingJob
- Interfaces: IMediaRepository, IImageProcessor, IVideoTranscoder
- Events: MediaUploadedEvent, ThumbnailGeneratedEvent, TranscodingCompletedEvent

### Infrastructure Layer
- SQL Server Express: `Server=.\\SQLEXPRESS;Database=MediaDb;Trusted_Connection=True;TrustServerCertificate=True`
- DbContext: MediaDbContext
- Libraries: ImageSharp (images), FFmpeg (video)

### API Layer
- Endpoints: POST /api/media/upload, GET /api/media/{id}, GET /api/media/{id}/thumbnail
- Support image formats: JPEG, PNG, GIF, WebP

## 3. Platform Tiers
- Basic: Simple image upload and retrieval
- Standard: Thumbnail generation, image resizing, format conversion
- Enterprise: Video transcoding, streaming support, CDN delivery, EXIF extraction, watermarking

## 4. Database Schema
- MediaFiles: MediaId PK, FileName, Type, Width, Height, SizeBytes, StoragePath, UploadedAt
- Thumbnails: ThumbnailId PK, MediaId FK, Width, Height, StoragePath

## Revision History
| Version | Date       | Changes |
|---------|------------|---------|
| 1.0     | 2026-01-12 | Initial specification |
