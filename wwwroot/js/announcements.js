document.addEventListener('DOMContentLoaded', function () {
    const markReadButtons = document.querySelectorAll('.mark-read-btn');
    markReadButtons.forEach(button => {
        button.addEventListener('click', async function () {
            const notificationId = this.getAttribute('data-notification-id');
            try {
                const response = await fetch('/Student/MarkNotificationRead', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json'
                    },
                    body: JSON.stringify({ notificationId: parseInt(notificationId) })
                });
                const result = await response.json();
                if (result.success) {
                    // تحديث واجهة المستخدم
                    this.parentElement.parentElement.parentElement.style.opacity = '0.7';
                    this.textContent = 'Read';
                    this.disabled = true;

                    // تحديث عدد الإشعارات غير المقروءة
                    const unreadBadge = document.querySelector('.badge.text-bg-primary');
                    if (unreadBadge) {
                        let count = parseInt(unreadBadge.textContent.split(':')[1].trim());
                        if (count > 0) {
                            count--;
                            unreadBadge.textContent = `Unread: ${count}`;
                        }
                    }
                } else {
                    console.error('Error:', result.message);
                }
            } catch (error) {
                console.error('Error:', error);
            }
        });
    });
});