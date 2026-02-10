# Ace Job Agency – Web Application Security

This README documents the security features implemented as part of the assignment
and serves as a security checklist and audit reference.

---

## Web Application Security Checklist

### Registration and User Data Management
- [] Implement successful saving of member info into the database
- [] Check for duplicate email addresses
- [] Strong password requirements (min 12 chars, complexity enforced)
- [] Client-side and server-side password validation
- [] Encrypt sensitive user data in the database (NRIC)
- [] Secure password hashing (ASP.NET Identity PasswordHasher)
- [] File upload restrictions (.pdf, .docx only)

### Session Management
- [] Secure session creation on login
- [] Session timeout handling
- [] Session cleared on logout

### Login / Logout Security
- [] Account lockout after multiple failed attempts
- [] Audit logging (login, logout, failed login)
- [] Secure logout handling

### Anti-Bot Protection
- [] Google reCAPTCHA v3 integration

### Input Validation and Sanitization
- [] CSRF protection (AntiForgeryToken)
- [] Server-side validation using DataAnnotations
- [] XSS and injection prevention via MVC model binding

### Error Handling
- [] Custom error pages (404, 403)
- [] Graceful exception handling

### Advanced Security Features
- [] Change password functionality
- [] Reset password via email token
- [] Minimum password age enforcement
- [] Maximum password age enforcement
- [] Password history enforcement
- [] Audit logging
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

