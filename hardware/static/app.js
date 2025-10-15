// Application State
const AppState = {
    ws: null,
    mode: null, // 'enter' or 'leave'
    room: 'pc1', // default room
    pendingCard: null,
    referenceImage: null, // Reference image from API
    stream: null,
    capturedImage: null,
    analysisResult: null
};

// DOM Elements
const elements = {
    mainButtons: document.getElementById('main-buttons'),
    enterBtn: document.getElementById('enter-btn'),
    leaveBtn: document.getElementById('leave-btn'),
    statusMessage: document.getElementById('status-message'),
    cardStage: document.getElementById('card-stage'),
    cameraStage: document.getElementById('camera-stage'),
    previewStage: document.getElementById('preview-stage'),
    resultStage: document.getElementById('result-stage'),
    cancelBtn: document.getElementById('cancel-btn'),
    video: document.getElementById('video'),
    canvas: document.getElementById('canvas'),
    captureBtn: document.getElementById('capture-btn'),
    cancelCameraBtn: document.getElementById('cancel-camera-btn'),
    previewImage: document.getElementById('preview-image'),
    analysisResult: document.getElementById('analysis-result'),
    confirmBtn: document.getElementById('confirm-btn'),
    retakeBtn: document.getElementById('retake-btn'),
    resultIcon: document.getElementById('result-icon'),
    resultTitle: document.getElementById('result-title'),
    resultMessage: document.getElementById('result-message'),
    resultDetails: document.getElementById('result-details'),
    doneBtn: document.getElementById('done-btn'),
    wsStatus: document.getElementById('ws-status'),
    wsText: document.getElementById('ws-text')
};

// Initialize WebSocket connection
function initWebSocket() {
    const protocol = window.location.protocol === 'https:' ? 'wss:' : 'ws:';
    const wsUrl = `${protocol}//${window.location.host}/ws?room=${AppState.room}`;
    
    AppState.ws = new WebSocket(wsUrl);
    
    AppState.ws.onopen = () => {
        console.log('WebSocket connected');
        updateConnectionStatus(true);
        showStatus('Connected to server', 'success');
    };
    
    AppState.ws.onclose = () => {
        console.log('WebSocket disconnected');
        updateConnectionStatus(false);
        showStatus('Disconnected from server', 'error');
        
        // Attempt to reconnect after 3 seconds
        setTimeout(() => {
            console.log('Attempting to reconnect...');
            initWebSocket();
        }, 3000);
    };
    
    AppState.ws.onerror = (error) => {
        console.error('WebSocket error:', error);
        showStatus('Connection error', 'error');
    };
    
    AppState.ws.onmessage = (event) => {
        handleWebSocketMessage(event.data);
    };
}

// Handle WebSocket messages
function handleWebSocketMessage(data) {
    console.log('Received:', data);
    
    // Try to parse as JSON
    try {
        const message = JSON.parse(data);
        handleJSONMessage(message);
    } catch (e) {
        // Plain text message
        showStatus(data, 'info');
    }
}

// Handle JSON messages from server
function handleJSONMessage(message) {
    const { event } = message;
    
    switch (event) {
        case 'card_read':
            // Only used for ENTER mode (leave/exit sends result immediately without camera)
            AppState.pendingCard = message.card;
            // Reference image is stored on server, just check if it exists
            const hasReference = message.card.hasReferenceImage;
            
            if (hasReference) {
                showStatus('Card detected! Reference image ready. Preparing camera...', 'success');
            } else {
                showStatus('Card detected! Warning: No reference image found.', 'warning');
            }
            
            hideStage('card');
            startCamera();
            break;
            
        case 'enter_timeout':
        case 'leave_timeout':
            showStatus('Card reading timeout. Please try again.', 'warning');
            resetToMain();
            break;
            
        case 'enter_cancelled':
        case 'leave_cancelled':
            showStatus('Operation cancelled', 'info');
            resetToMain();
            break;
            
        case 'enter_done':
            handleApiResponse(true, message.result);
            break;
            
        case 'leave_done':
            handleApiResponse(false, message.result);
            break;
            
        case 'enter_error':
        case 'leave_error':
            handleApiResponse(event === 'enter_error', message.result, true);
            break;
            
        case 'error':
            showStatus(`Error: ${message.message}`, 'error');
            resetToMain();
            break;
            
        case 'cancelled':
            showStatus('Operation cancelled', 'info');
            resetToMain();
            break;
            
        default:
            console.log('Unhandled event:', event, message);
    }
}

// Update connection status indicator
function updateConnectionStatus(connected) {
    if (connected) {
        elements.wsStatus.className = 'status-indicator connected';
        elements.wsText.textContent = 'Connected';
    } else {
        elements.wsStatus.className = 'status-indicator disconnected';
        elements.wsText.textContent = 'Disconnected';
    }
}

// Show status message
function showStatus(message, type = 'info') {
    elements.statusMessage.textContent = message;
    elements.statusMessage.className = `status-message ${type}`;
}

// Hide status message
function hideStatus() {
    elements.statusMessage.textContent = '';
    elements.statusMessage.className = 'status-message';
}

// Show/hide stages
function showStage(stage) {
    hideAllStages();
    
    switch (stage) {
        case 'card':
            elements.cardStage.classList.remove('hidden');
            break;
        case 'camera':
            elements.cameraStage.classList.remove('hidden');
            break;
        case 'preview':
            elements.previewStage.classList.remove('hidden');
            break;
        case 'result':
            elements.resultStage.classList.remove('hidden');
            break;
    }
}

function hideStage(stage) {
    switch (stage) {
        case 'card':
            elements.cardStage.classList.add('hidden');
            break;
        case 'camera':
            elements.cameraStage.classList.add('hidden');
            break;
        case 'preview':
            elements.previewStage.classList.add('hidden');
            break;
        case 'result':
            elements.resultStage.classList.add('hidden');
            break;
    }
}

function hideAllStages() {
    elements.cardStage.classList.add('hidden');
    elements.cameraStage.classList.add('hidden');
    elements.previewStage.classList.add('hidden');
    elements.resultStage.classList.add('hidden');
}

// Enable/disable main buttons
function setMainButtonsEnabled(enabled) {
    elements.enterBtn.disabled = !enabled;
    elements.leaveBtn.disabled = !enabled;
}

// Enter button handler
elements.enterBtn.addEventListener('click', () => {
    AppState.mode = 'enter';
    setMainButtonsEnabled(false);
    showStatus('Initializing enter mode...', 'info');
    showStage('card');
    
    if (AppState.ws && AppState.ws.readyState === WebSocket.OPEN) {
        AppState.ws.send('enter');
    } else {
        showStatus('Not connected to server', 'error');
        resetToMain();
    }
});

// Leave button handler
elements.leaveBtn.addEventListener('click', () => {
    AppState.mode = 'leave';
    setMainButtonsEnabled(false);
    showStatus('Initializing leave mode...', 'info');
    showStage('card');
    
    if (AppState.ws && AppState.ws.readyState === WebSocket.OPEN) {
        AppState.ws.send('leave');
    } else {
        showStatus('Not connected to server', 'error');
        resetToMain();
    }
});

// Cancel button handler (during card reading)
elements.cancelBtn.addEventListener('click', () => {
    if (AppState.ws && AppState.ws.readyState === WebSocket.OPEN) {
        AppState.ws.send('cancel');
    }
    resetToMain();
});

// Start camera
async function startCamera() {
    try {
        AppState.stream = await navigator.mediaDevices.getUserMedia({ 
            video: { 
                width: { ideal: 1280 },
                height: { ideal: 720 },
                facingMode: 'user'
            } 
        });
        elements.video.srcObject = AppState.stream;
        showStage('camera');
        showStatus('Position yourself for photo verification', 'info');
    } catch (error) {
        console.error('Error accessing camera:', error);
        showStatus('Could not access camera. Please check permissions.', 'error');
        resetToMain();
    }
}

// Stop camera
function stopCamera() {
    if (AppState.stream) {
        AppState.stream.getTracks().forEach(track => track.stop());
        AppState.stream = null;
        elements.video.srcObject = null;
    }
}

// Capture photo button handler
elements.captureBtn.addEventListener('click', async () => {
    const context = elements.canvas.getContext('2d');
    
    // Set canvas dimensions to match video
    elements.canvas.width = elements.video.videoWidth;
    elements.canvas.height = elements.video.videoHeight;
    
    // Draw video frame to canvas
    context.drawImage(elements.video, 0, 0, elements.canvas.width, elements.canvas.height);
    
    // Get data URL
    AppState.capturedImage = elements.canvas.toDataURL('image/jpeg', 0.9);
    
    // Stop camera
    stopCamera();
    
    // Show preview
    elements.previewImage.src = AppState.capturedImage;
    showStage('preview');
    
    // Send image to server for analysis
    showStatus('Analyzing photo...', 'info');
    elements.analysisResult.textContent = 'Analyzing face... Please wait.';
    elements.analysisResult.className = 'analysis-result pending';
    elements.confirmBtn.disabled = true;
    
    try {
        const response = await fetch('/capture', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                room: AppState.room,
                image: AppState.capturedImage
                // referenceImage is stored on server, not sent from frontend
            })
        });
        
        const result = await response.json();
        AppState.analysisResult = result;
        
        console.log('Capture result:', result);
        
        if (result.status === 'ok' && result.compare) {
            const compare = result.compare;
            console.log('Compare result:', compare);
            
            if (compare.status === 'ok') {
                const similarity = (compare.similarity * 100).toFixed(1);
                if (compare.match) {
                    elements.analysisResult.innerHTML = `✓ Face verified! (${similarity}% match)`;
                    elements.analysisResult.className = 'analysis-result match';
                    showStatus('Face verification successful', 'success');
                    elements.confirmBtn.disabled = false; // Enable confirm only on match
                } else {
                    elements.analysisResult.innerHTML = `⚠ Face verification failed (${similarity}% match)<br><small>Cannot proceed - faces don't match</small>`;
                    elements.analysisResult.className = 'analysis-result no-match';
                    showStatus('Face verification failed - similarity too low', 'warning');
                    elements.confirmBtn.disabled = true; // Disable confirm on no match
                }
            } else {
                elements.analysisResult.innerHTML = `⚠ ${compare.message || 'Analysis error'}<br><small>Cannot proceed - analysis failed</small>`;
                elements.analysisResult.className = 'analysis-result no-match';
                showStatus('Face analysis error: ' + compare.message, 'warning');
                elements.confirmBtn.disabled = true; // Disable confirm on error
            }
        } else {
            const errorMsg = result.compare ? result.compare.message : 'No comparison data';
            elements.analysisResult.innerHTML = `⚠ Analysis unavailable: ${errorMsg}<br><small>Cannot proceed</small>`;
            elements.analysisResult.className = 'analysis-result pending';
            showStatus('Photo captured, analysis unavailable', 'warning');
            console.error('Analysis unavailable:', result);
            elements.confirmBtn.disabled = true; // Disable confirm when no comparison data
        }
        
    } catch (error) {
        console.error('Error analyzing photo:', error);
        elements.analysisResult.innerHTML = '⚠ Analysis failed<br><small>Cannot proceed - try again</small>';
        elements.analysisResult.className = 'analysis-result no-match';
        showStatus('Analysis error occurred', 'error');
        elements.confirmBtn.disabled = true; // Disable confirm on network/analysis error
    }
});

// Cancel camera button handler
elements.cancelCameraBtn.addEventListener('click', () => {
    stopCamera();
    resetToMain();
});

// Confirm button handler (after photo capture)
elements.confirmBtn.addEventListener('click', () => {
    if (AppState.ws && AppState.ws.readyState === WebSocket.OPEN) {
        const command = AppState.mode === 'enter' ? 'enter_confirm' : 'leave_confirm';
        AppState.ws.send(command);
        showStatus('Processing request...', 'info');
        elements.confirmBtn.disabled = true;
        elements.retakeBtn.disabled = true;
    } else {
        showStatus('Not connected to server', 'error');
        resetToMain();
    }
});

// Retake button handler
elements.retakeBtn.addEventListener('click', () => {
    hideStage('preview');
    startCamera();
});

// Handle API response with detailed user-friendly messages
function handleApiResponse(isEnter, result, isError = false) {
    if (!result) {
        showResult(false, 'Error', 'No response from server', null);
        return;
    }
    
    // Extract the message from the response object
    const apiMessage = result.response?.message || result.message || '';
    const mode = isEnter ? 'Entry' : 'Exit';
    
    console.log('handleApiResponse:', { isEnter, result, apiMessage, isError });
    
    // Success messages
    const successMessages = {
        'Authorized': {
            title: `${mode} Authorized ✓`,
            message: isEnter 
                ? `Welcome! Your entry has been recorded.` 
                : `Goodbye! Your exit has been recorded.`,
            icon: '✓',
            success: true
        },
        'AuthorizedAsAdmin': {
            title: `${mode} Authorized (Admin) ✓`,
            message: isEnter
                ? `Admin access granted. Your entry has been recorded.`
                : `Admin exit recorded. Goodbye!`,
            icon: '✓',
            success: true
        },
        'ExamAttendanceRecorded': {
            title: 'Exam Attendance Recorded ✓',
            message: result.status === 'Approved' 
                ? 'You arrived on time for the exam. Good luck!' 
                : 'You arrived late for the exam. Your attendance has been marked as late.',
            icon: '✓',
            success: true,
            details: result.status ? `Status: ${result.status}` : null
        },
        'ExitRecorded': {
            title: 'Exit Recorded ✓',
            message: 'Your departure has been successfully recorded. Goodbye!',
            icon: '✓',
            success: true
        }
    };
    
    // Error messages
    const errorMessages = {
        'InvalidForm': {
            title: 'Invalid Request',
            message: 'The request format was invalid. Please try again.',
            icon: '⚠',
            success: false
        },
        'KeycardNotFound': {
            title: 'Card Not Recognized',
            message: 'Your keycard is not registered in the system. Please contact administration.',
            icon: '❌',
            success: false
        },
        'RoomNotFound': {
            title: 'Room Not Found',
            message: 'The room you are trying to access is not in the system.',
            icon: '❌',
            success: false
        },
        'UserAlreadyInRoom': {
            title: 'Already Checked In',
            message: 'You are already checked into a room. Please exit first before entering another room.',
            icon: '⚠',
            success: false
        },
        'DeniedByTeacher': {
            title: 'Access Denied',
            message: 'Your exam entry request was denied by the teacher.',
            icon: '🚫',
            success: false
        },
        'WaitForResponse': {
            title: 'Waiting for Approval',
            message: 'Your exam entry request is pending. Please wait for teacher approval.',
            icon: '⏳',
            success: false
        },
        'NotEnrolledInExamCourse': {
            title: 'Not Enrolled',
            message: 'You are not enrolled in the course for this exam. Entry not allowed.',
            icon: '🚫',
            success: false
        },
        'NoCourseInRoomToday': {
            title: 'No Course Today',
            message: 'You do not have a scheduled course in this room today.',
            icon: '📅',
            success: false
        },
        'NoCourseScheduledToday': {
            title: 'Not Scheduled',
            message: 'No course is scheduled for you in this room at this time.',
            icon: '📅',
            success: false
        },
        'CourseAlreadyEnded': {
            title: 'Course Ended',
            message: 'The course in this room has already ended. Entry not allowed.',
            icon: '⏰',
            success: false
        },
        'CourseSubjectMissing': {
            title: 'System Error',
            message: 'Course data is incomplete. Please contact administration.',
            icon: '⚠',
            success: false
        }
    };
    
    // Check if we have a predefined message
    let messageConfig = successMessages[apiMessage] || errorMessages[apiMessage];
    
    // If no predefined message, create a generic one
    if (!messageConfig) {
        messageConfig = {
            title: isError ? 'Operation Failed' : `${mode} Complete`,
            message: isError 
                ? `An error occurred: ${apiMessage}` 
                : `Your ${mode.toLowerCase()} has been processed.`,
            icon: isError ? '❌' : '✓',
            success: !isError,
            details: apiMessage
        };
    }
    
    // Show attendance type for successful course entries
    let detailsText = messageConfig.details || '';
    const attendanceType = result.response?.attendanceType || result.attendanceType;
    const examStatus = result.response?.status || result.status;
    
    if (messageConfig.success && attendanceType) {
        const attendanceStatus = attendanceType === 'Late' 
            ? '⏰ Status: Arrived Late' 
            : '✓ Status: On Time';
        detailsText = detailsText ? `${detailsText}\n${attendanceStatus}` : attendanceStatus;
    }
    
    if (messageConfig.success && examStatus && apiMessage === 'ExamAttendanceRecorded') {
        const statusText = `📝 Exam Status: ${examStatus}`;
        detailsText = detailsText ? `${detailsText}\n${statusText}` : statusText;
    }
    
    // Build HTTP log details
    let httpLog = '';
    if (result) {
        // Request details
        const requestPayload = result.entry || result.leave;
        if (requestPayload) {
            httpLog += '📤 REQUEST:\n';
            httpLog += `Endpoint: ${isEnter ? '/api/Keys/enter' : '/api/Keys/exit'}\n`;
            httpLog += `Method: POST\n`;
            httpLog += `Payload: ${JSON.stringify(requestPayload, null, 2)}\n\n`;
        }
        
        // Response details
        if (result.code !== undefined) {
            httpLog += '📥 RESPONSE:\n';
            httpLog += `Status Code: ${result.code}\n`;
            
            if (result.response) {
                if (typeof result.response === 'object') {
                    httpLog += `Body:\n${JSON.stringify(result.response, null, 2)}`;
                } else {
                    httpLog += `Body: ${result.response}`;
                }
            }
        }
    }
    
    // Combine details text with HTTP log
    const finalDetails = detailsText 
        ? `${detailsText}\n\n${'─'.repeat(40)}\n\n${httpLog}` 
        : httpLog;
    
    showResult(
        messageConfig.success, 
        messageConfig.title, 
        messageConfig.message,
        finalDetails || null
    );
}

// Show result
function showResult(success, title, message, details) {
    console.log('showResult called:', { success, title, message, details });
    elements.resultIcon.className = success ? 'result-icon success' : 'result-icon error';
    elements.resultTitle.textContent = title;
    elements.resultMessage.textContent = message;
    
    if (details) {
        console.log('Setting details:', details);
        // If details is a string, show it as text; otherwise show as JSON
        if (typeof details === 'string') {
            elements.resultDetails.innerHTML = `<div style="white-space: pre-wrap; font-family: 'Courier New', monospace; font-size: 0.85rem; line-height: 1.5; color: #333;">${details}</div>`;
        } else {
            elements.resultDetails.innerHTML = `<pre>${JSON.stringify(details, null, 2)}</pre>`;
        }
        console.log('resultDetails innerHTML:', elements.resultDetails.innerHTML);
    } else {
        console.log('No details to show');
        elements.resultDetails.innerHTML = '';
    }
    
    showStage('result');
    hideStatus();
}

// Done button handler (after result)
elements.doneBtn.addEventListener('click', () => {
    resetToMain();
});

// Reset to main screen
function resetToMain() {
    stopCamera();
    hideAllStages();
    hideStatus();
    setMainButtonsEnabled(true);
    AppState.mode = null;
    AppState.pendingCard = null;
    AppState.referenceImage = null;
    AppState.capturedImage = null;
    AppState.analysisResult = null;
    elements.confirmBtn.disabled = false;
    elements.retakeBtn.disabled = false;
}

// Initialize on page load
document.addEventListener('DOMContentLoaded', () => {
    console.log('Initializing Card Reader System...');
    initWebSocket();
});
