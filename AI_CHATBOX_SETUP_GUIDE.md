# ArtJourney AI Chatbox Setup Guide

## Overview
This guide will help you set up the AI-powered learning assistant for your ArtJourney platform. The chatbox provides personalized art history guidance using LM Studio and local AI models.

## Prerequisites

### 1. LM Studio Setup
1. Download and install [LM Studio](https://lmstudio.ai/)
2. Recommended models for your use case:
   - **Primary: Qwen2.5-7B-Instruct** (Best for educational guidance)
   - **Alternative: Llama 3.2-3B-Instruct** (Lighter, faster responses)
   - **Advanced: Llama 3.1-8B-Instruct** (Better reasoning)

### 2. Model Configuration
1. Open LM Studio
2. Download your chosen model
3. Go to "Local Server" tab
4. Select your model
5. Configure settings:
   - **Port**: 1234 (default)
   - **Context Length**: 4096 or higher
   - **Temperature**: 0.7
   - **Max Tokens**: 1500

### 3. Start Local Server
1. Click "Start Server" in LM Studio
2. Verify server is running at `http://127.0.0.1:1234`
3. Test with: `GET http://127.0.0.1:1234/v1/models`

## Backend Configuration

### 1. Configuration Settings
The AI service is configured in `appsettings.json`:

```json
{
  "AIService": {
    "BaseUrl": "http://127.0.0.1:1234/v1",
    "ModelName": "qwen2.5-7b-instruct",
    "MaxTokens": 1500,
    "Temperature": 0.7,
    "ContextLength": 10,
    "Timeout": 120
  }
}
```

### 2. Available Endpoints

#### Chat Endpoints
- `POST /api/chat/message` - Send message to AI
- `POST /api/chat/session` - Create new chat session
- `GET /api/chat/sessions` - Get user's chat sessions
- `GET /api/chat/session/{id}` - Get specific chat session
- `DELETE /api/chat/session/{id}` - Deactivate chat session

#### Context and Analytics
- `GET /api/chat/user-context` - Get user learning context
- `GET /api/chat/learning-analytics` - Get comprehensive analytics
- `GET /api/chat/system-prompt` - Get current AI prompt (debugging)
- `GET /api/chat/ai-status` - Check AI service status

### 3. Message Request Format
```json
{
  "message": "What should I study next in Renaissance art?",
  "chatSessionId": 123,
  "includeUserProgress": true
}
```

### 4. Response Format
```json
{
  "status": "Success",
  "code": 200,
  "data": {
    "chatMessageId": 456,
    "chatSessionId": 123,
    "role": "assistant",
    "content": "Based on your progress in Italian Renaissance...",
    "timestamp": "2024-06-28T10:30:00Z",
    "modelUsed": "LM Studio Local",
    "responseTime": 2.5
  },
  "message": "Message sent successfully"
}
```

## AI Capabilities

### 1. Personalized Guidance
- Analyzes user's current course progress
- Identifies areas needing focus
- Suggests personalized study plans
- Tracks learning streaks and motivation

### 2. Learning Analytics
- **Course Progress**: Real-time progress tracking
- **Learning Patterns**: Study time preferences, session duration
- **Performance Metrics**: Scores, completion rates, weak areas
- **Recommendations**: Next topics, review areas, study schedules

### 3. Contextual Responses
The AI has access to:
- User's enrolled courses and progress
- Recently completed topics
- Struggle areas and high-performing topics
- Learning style preferences
- Study streaks and engagement patterns

## Frontend Integration

### 1. Basic Chat Implementation
```javascript
// Send message to AI
const sendMessage = async (message, sessionId = null) => {
  const response = await fetch('/api/chat/message', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${userToken}`
    },
    body: JSON.stringify({
      message: message,
      chatSessionId: sessionId,
      includeUserProgress: true
    })
  });
  
  return await response.json();
};

// Get user learning analytics
const getAnalytics = async () => {
  const response = await fetch('/api/chat/learning-analytics', {
    headers: {
      'Authorization': `Bearer ${userToken}`
    }
  });
  
  return await response.json();
};

// Check AI service status
const checkAIStatus = async () => {
  const response = await fetch('/api/chat/ai-status');
  return await response.json();
};
```

### 2. Real-time Features
- All timestamps are in UTC for consistency
- Chat history maintains context (configurable length)
- Session management for conversation continuity
- Background service health monitoring

## Testing and Debugging

### 1. Test AI Service Connection
```bash
# Check if LM Studio is running
curl http://127.0.0.1:1234/v1/models

# Test through your API
curl -X GET "http://localhost:8080/api/chat/ai-status" \
  -H "Authorization: Bearer YOUR_TOKEN"
```

### 2. Test Chat Functionality
```bash
# Send a test message
curl -X POST "http://localhost:8080/api/chat/message" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -d '{
    "message": "Hello, can you help me with my art history studies?",
    "includeUserProgress": true
  }'
```

### 3. Monitor Logs
The system logs AI service interactions:
- Request/response times
- Model performance
- Error handling
- User context retrieval

## Customization Options

### 1. Model Selection
- **Qwen2.5-7B**: Best balance of capability and speed
- **Llama 3.1-8B**: Better for complex reasoning
- **Mistral-7B**: Good alternative with different personality

### 2. System Prompt Customization
The AI personality can be modified in `AIService.BuildSystemPrompt()`:
- Adjust tone and personality
- Modify response guidelines
- Change educational focus areas
- Update recommendation strategies

### 3. Context Length
Adjust `ContextLength` in configuration to control:
- How much chat history to include
- Memory of conversation context
- Performance vs. context trade-off

## Performance Optimization

### 1. Model Performance
- **3B models**: ~2-4 seconds response time
- **7B models**: ~3-6 seconds response time
- **8B+ models**: ~5-10 seconds response time

### 2. Optimization Tips
- Use GPU acceleration in LM Studio if available
- Adjust `max_tokens` based on typical response length needs
- Monitor response times and adjust timeout settings
- Consider model quantization for faster inference

## Deployment Considerations

### 1. Production Setup
- Consider using a dedicated GPU server for AI inference
- Implement proper error handling and fallback responses
- Set up monitoring for AI service availability
- Configure appropriate timeout values

### 2. Alternative Deployment Options
- **Docker**: Containerize LM Studio with your models
- **Cloud**: Use services like Hugging Face Inference API
- **Tailscale**: For secure remote access to local AI server
- **Load Balancing**: Multiple AI service instances for scale

## Troubleshooting

### Common Issues
1. **AI Service Not Available**: Check LM Studio is running and accessible
2. **Slow Responses**: Reduce model size or increase timeout
3. **Memory Issues**: Use quantized models or reduce context length
4. **Connection Errors**: Verify firewall settings and port availability

### Error Handling
The system includes comprehensive error handling:
- Graceful fallback responses when AI is unavailable
- Timeout handling for slow responses
- Automatic retry logic for transient failures
- User-friendly error messages

This setup provides a robust, personalized AI learning assistant that will significantly enhance the educational experience on your ArtJourney platform! 🎨✨
