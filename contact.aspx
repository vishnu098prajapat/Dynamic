<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true" CodeFile="contact.aspx.cs" Inherits="contact" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">
     <style>
    /* Contact Page Specific Styles */
    .contact-hero {
      padding: 140px 0 60px;
      text-align: center;
      background: var(--bg-section);
      position: relative;
    }
    .contact-badge {
      display: inline-block;
      padding: 0.5rem 1.5rem;
      background: var(--surface);
      border: 1px solid var(--border-color);
      border-radius: 30px;
      color: var(--gold);
      font-weight: 600;
      margin-bottom: 1.5rem;
      font-size: 0.9rem;
    }
    .contact-hero h1 {
      font-size: 3.5rem;
      margin-bottom: 1rem;
    }
    .contact-hero p {
      font-size: 1.1rem;
      color: var(--text-body);
      max-width: 600px;
      margin: 0 auto;
    }
    
    .contact-cards {
      display: grid;
      grid-template-columns: repeat(4, 1fr);
      gap: 1.5rem;
      margin-top: 4rem;
    }
    .contact-card {
      background: var(--surface);
      border: 1px solid var(--border-color);
      border-radius: 16px;
      padding: 2.5rem 1.5rem;
      text-align: center;
      transition: transform 0.3s, box-shadow 0.3s;
    }
    .contact-card:hover {
      transform: translateY(-5px);
      box-shadow: 0 20px 40px rgba(0,0,0,0.05);
      border-color: var(--gold);
    }
    .c-icon {
      width: 60px; height: 60px;
      background: var(--gold-glow);
      color: var(--gold);
      border-radius: 50%;
      display: flex; align-items: center; justify-content: center;
      font-size: 1.8rem;
      margin: 0 auto 1.5rem;
    }
    .contact-card h3 {
      font-size: 1.2rem;
      color: var(--gold);
      margin-bottom: 1rem;
    }
    .contact-card p {
      font-size: 0.95rem;
      color: var(--text-body);
      line-height: 1.6;
    }

    .contact-content {
      display: grid;
      grid-template-columns: 1fr 1fr;
      gap: 4rem;
      padding: 5rem 0;
    }
    .contact-form-box {
      background: var(--surface);
      padding: 3rem;
      border-radius: 24px;
      border: 1px solid var(--border-color);
      box-shadow: 0 30px 60px rgba(0,0,0,0.05);
    }
    .contact-form-box h2 {
      font-size: 2.2rem;
      margin-bottom: 0.5rem;
      color: var(--text-head);
    }
    .contact-form-box > p {
      color: var(--text-muted);
      margin-bottom: 2.5rem;
    }

    .form-group {
      margin-bottom: 1.5rem;
    }
    .form-row {
      display: grid;
      grid-template-columns: 1fr 1fr;
      gap: 1.5rem;
    }
    label {
      display: block;
      font-weight: 600;
      margin-bottom: 0.5rem;
      color: var(--text-head);
      font-size: 0.95rem;
    }
    input, select, textarea {
      width: 100%;
      padding: 1rem 1.2rem;
      border: 1px solid var(--border-color);
      border-radius: 12px;
      background: var(--bg-light);
      color: var(--text-head);
      font-family: inherit;
      transition: all 0.3s;
      font-size: 1rem;
    }
    input:focus, select:focus, textarea:focus {
      outline: none;
      border-color: var(--gold);
      background: var(--surface);
      box-shadow: 0 0 0 4px var(--gold-glow);
    }
    .map-container {
      border-radius: 24px;
      overflow: hidden;
      border: 1px solid var(--border-color);
      height: 100%;
      min-height: 500px;
    }
    
    @media (max-width: 992px) {
      .contact-cards { grid-template-columns: repeat(2, 1fr); }
      .contact-content { grid-template-columns: 1fr; }
      .map-container { min-height: 400px; }
    }
    @media (max-width: 768px) {
      .contact-cards { grid-template-columns: 1fr; }
      .form-row { grid-template-columns: 1fr; }
      .contact-hero h1 { font-size: 2.5rem; }
      .contact-form-box { padding: 2rem; }
    }
  </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">
    <section class="contact-hero">
    <div class="container">
      <div class="contact-badge">💬 We're Here to Help</div>
      <h1 class="font-playfair">Get In Touch With <span style="color:var(--gold);"><%=company.Name %></span></h1>
      <p>Whether you want to become a retail partner, need technical assistance, or have a general inquiry — our team is ready to assist you.</p>
      
      <div class="contact-cards">
        <div class="contact-card">
          <div class="c-icon">📍</div>
          <h3>Our Office</h3>
          <p><%=company.Address %></p>
        </div>
        <div class="contact-card">
          <div class="c-icon">📞</div>
          <h3>Call Us</h3>
          <p>+91 <%=company.Mobile %><br>Mon - Sat: 10 AM - 7 PM IST</p>
        </div>
        <div class="contact-card">
          <div class="c-icon">✉️</div>
          <h3>Email Support</h3>
          <p><%=company.Email %><br>We reply within 24 hours</p>
        </div>
        <div class="contact-card">
          <div class="c-icon">⏰</div>
          <h3>Business Hours</h3>
          <p>Monday – Saturday<br>10:00 AM to 7:00 PM IST</p>
        </div>
      </div>
    </div>
  </section>

  <section style="background: var(--bg-light);">
    <div class="container contact-content">
      
      <!-- Contact Form -->
      <div class="contact-form-box">
        <h2 class="font-playfair">Send Us a Message</h2>
        <p>Fill in the details below and our team will get back to you within 24 hours.</p>
        
        <form>
          <div class="form-row">
            <div class="form-group">
              <label>Full Name *</label>
              <input type="text" placeholder="e.g. Ramesh Kumar" required>
            </div>
            <div class="form-group">
              <label>Email Address *</label>
              <input type="email" placeholder="e.g. ramesh@example.com" required>
            </div>
          </div>
          
          <div class="form-row">
            <div class="form-group">
              <label>Phone Number</label>
              <input type="tel" placeholder="+91 <%=company.Mobile %>">
            </div>
            <div class="form-group">
              <label>Subject *</label>
              <select required>
                <option value="" disabled selected>Select a subject...</option>
                <option value="retailer">Become a Retailer</option>
                <option value="distributor">Become a Distributor</option>
                <option value="support">Technical Support</option>
                <option value="other">Other Inquiry</option>
              </select>
            </div>
          </div>
          
          <div class="form-group">
            <label>Your Message *</label>
            <textarea rows="5" placeholder="Describe your query, requirement, or feedback..." required></textarea>
          </div>
          
          <button type="submit" class="btn-primary" style="width: 100%; padding: 1.2rem; font-size: 1.1rem; border-radius: 12px; margin-top: 1rem;">Send Message</button>
        </form>
      </div>
      
      <!-- Map -->
      <div class="map-container">
        <!-- Using a generic map embed for Jaipur (since the specific location is just illustrative) -->
        <iframe 
          src='<%=company.Map %>' 
          width="100%" 
          height="100%" 
          style="border:0;" 
          allowfullscreen="" 
          loading="lazy" 
          referrerpolicy="no-referrer-when-downgrade">
        </iframe>
      </div>

    </div>
  </section>
</asp:Content>

