# Ace Job Agency – Web Application Security

This README documents the security features implemented as part of the assignment
and serves as a security checklist and audit reference.

---

## Web Application Security Checklist

### Registration and User Data Management
- [x] Implement successful saving of member info into the database
- [x] Check for duplicate email addresses
- [x] Strong password requirements (min 12 chars, complexity enforced)
- [x] Client-side and server-side password validation
- [x] Encrypt sensitive user data in the database (NRIC)
- [x] Secure password hashing (ASP.NET Identity PasswordHasher)
- [x] File upload restrictions (.pdf, .docx only)

### Session Management
- [x] Secure session creation on login
- [x] Session timeout handling
- [x] Session cleared on logout

### Login / Logout Security
- [x] Account lockout after multiple failed attempts
- [x] Audit logging (login, logout, failed login)
- [x] Secure logout handling

### Anti-Bot Protection
- [x] Google reCAPTCHA v3 integration

### Input Validation and Sanitization
- [x] CSRF protection (AntiForgeryToken)
- [x] Server-side validation using DataAnnotations
- [x] XSS and injection prevention via MVC model binding

### Error Handling
- [x] Custom error pages (404, 403)
- [x] Graceful exception handling

### Advanced Security Features
- [x] Change password functionality
- [x] Reset password via email token
- [x] Minimum password age enforcement
- [x] Maximum password age enforcement
- [x] Password history enforcement
- [x] Audit logging
- [ ] Two-Factor Authentication 

---

## Security Testing
- GitHub Advanced Security
- Dependabot dependency scanning
- CodeQL static analysis

---

## Notes
- Sensitive Data, being NRIC is encrypted before storage
- Tokens are time-limited and single-use

