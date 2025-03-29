document.addEventListener('DOMContentLoaded', function() {
    const chatMessages = document.getElementById('chatMessages');
    const userInput = document.getElementById('userInput');
    const sendButton = document.getElementById('sendButton');

    // Các câu trả lời mẫu cho chatbot khi chưa có kết nối AI
    const responses = {
        greeting: [
            "Xin chào! Tôi có thể giúp gì cho bạn?",
            "Chào bạn! Bạn cần tư vấn gì về nông nghiệp không?",
            "Xin chào! Tôi là trợ lý nông nghiệp, tôi có thể giúp gì cho bạn?"
        ],
        default: [
            "Tôi đang xử lý câu hỏi của bạn...",
            "Để tôi tìm hiểu thêm về vấn đề này...",
            "Tôi sẽ trả lời bạn ngay..."
        ]
    };

    // Hàm thêm tin nhắn vào chat
    function addMessage(message, isUser = false) {
        const messageDiv = document.createElement('div');
        messageDiv.className = `message ${isUser ? 'user' : 'bot'}`;
        
        const messageContent = document.createElement('div');
        messageContent.className = 'message-content';
        messageContent.textContent = message;
        
        messageDiv.appendChild(messageContent);
        chatMessages.appendChild(messageDiv);
        
        // Cuộn xuống tin nhắn mới nhất
        chatMessages.scrollTop = chatMessages.scrollHeight;
    }

    // Hàm gọi API Groq Cloud
    async function callAI(message) {
        try {
            const response = await fetch('https://api.groq.com/v1/chat/completions', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': 'Bearer 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'
                },
                body: JSON.stringify({
                    model: "mixtral-8x7b-32768",
                    messages: [
                        {
                            role: "system",
                            content: "Bạn là một trợ lý AI chuyên về nông nghiệp, có tên là 'Chăm sóc nhà nông'. Bạn sẽ trả lời các câu hỏi liên quan đến nông nghiệp, canh tác, thời tiết, và các vấn đề khác liên quan đến nông nghiệp. Hãy trả lời ngắn gọn, dễ hiểu và hữu ích."
                        },
                        {
                            role: "user",
                            content: message
                        }
                    ],
                    temperature: 0.7,
                    max_tokens: 500,
                    stream: false
                })
            });

            if (!response.ok) {
                const errorData = await response.json();
                console.error('API Error:', errorData);
                throw new Error(`API Error: ${response.status} ${response.statusText}`);
            }

            const data = await response.json();
            
            if (!data.choices || !data.choices[0] || !data.choices[0].message) {
                console.error('Invalid API Response:', data);
                throw new Error('Invalid API Response Format');
            }

            return data.choices[0].message.content;
        } catch (error) {
            console.error('Error:', error);
            return "Xin lỗi, tôi đang gặp sự cố kết nối. Vui lòng thử lại sau.";
        }
    }

    // Hàm xử lý tin nhắn từ người dùng
    async function handleUserMessage(message) {
        addMessage(message, true);
        
        // Hiển thị tin nhắn đang xử lý
        const loadingMessage = "Tôi đang xử lý câu hỏi của bạn...";
        addMessage(loadingMessage);
        
        try {
            // Gọi API AI để xử lý câu hỏi
            const aiResponse = await callAI(message);
            
            // Xóa tin nhắn đang xử lý
            chatMessages.removeChild(chatMessages.lastChild);
            
            // Hiển thị câu trả lời từ AI
            addMessage(aiResponse);
        } catch (error) {
            // Xóa tin nhắn đang xử lý
            chatMessages.removeChild(chatMessages.lastChild);
            
            // Hiển thị thông báo lỗi
            addMessage("Xin lỗi, tôi đang gặp sự cố. Vui lòng thử lại sau.");
        }
    }

    // Xử lý sự kiện gửi tin nhắn
    function sendMessage() {
        const message = userInput.value.trim();
        if (message) {
            handleUserMessage(message);
            userInput.value = '';
        }
    }

    // Thêm sự kiện click cho nút gửi
    sendButton.addEventListener('click', sendMessage);
    
    // Thêm sự kiện Enter cho input
    userInput.addEventListener('keypress', function(e) {
        if (e.key === 'Enter') {
            sendMessage();
        }
    });
}); 