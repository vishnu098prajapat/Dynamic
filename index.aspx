<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true" CodeFile="index.aspx.cs" Inherits="index" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">
     <!-- HERO SECTION -->
  <section class="hero" id="home">
    <div class="hero-bg-elements">
      <div class="orb-1"></div>
      <div class="orb-2"></div>
      <div class="grid-overlay"></div>
    </div>
    <div class="container hero-container">
      <div class="hero-content">
        <div class="hero-badge">
          <span>⚡</span> India's #1 B2B Fintech Platform
        </div>
        <h1 id="heroTitle">
          <!-- JS will wrap words for animation -->
          Power Your Business With Digital Finance
        </h1>
        <p>Unlock multiple revenue streams with our unified platform for Recharges, Bill Payments, and Banking Services.<span class="typewriter-cursor"></span></p>
        <div class="hero-btns">
          <button class="btn-primary magnetic-btn">Get Started &rarr;</button>
          <button class="btn-secondary">Learn More &#9654;</button>
        </div>
        <div class="hero-trust">
          <div class="avatars">
            <div class="avatar">RS</div>
            <div class="avatar">MK</div>
            <div class="avatar">AP</div>
            <div class="avatar">+</div>
          </div>
          <div class="trust-text">10,000+ retailers trust us</div>
          <div class="operator-chips">
            <span class="chip">Airtel</span>
            <span class="chip">Jio</span>
            <span class="chip">VI</span>
            <span class="chip">BSNL</span>
          </div>
        </div>
      </div>
      
      <div class="hero-visual" id="heroVisual">
        <div class="hero-image-card" style="background: var(--surface); padding: 1rem; border-radius: 24px; box-shadow: 0 30px 80px rgba(201,168,76,0.15); border: 1px solid var(--border-color); width: 100%; z-index: 10; position: relative;">
          <img src="siteadmin/images/hero.png" alt="<%=company.Name %> Platform Interface" style="width: 100%; height: auto; border-radius: 16px; display: block;">
        </div>
      </div>
    </div>
    <div class="scroll-indicator">
      <div class="chevron"></div>
      Scroll to explore
    </div>
  </section>

  <!-- STATS MARQUEE -->
  <section class="stats-section">
    <div class="stats-bg"></div>
    <div class="marquee-container marquee-row-1">
      <div class="marquee-content" id="marquee1">
        <span class="marquee-item">10,000+ Retailers</span><span class="marquee-item">◆</span>
        <span class="marquee-item">&#8377;50Cr+ Processed</span><span class="marquee-item">◆</span>
        <span class="marquee-item">7 Services</span><span class="marquee-item">◆</span>
        <span class="marquee-item">99.9% Uptime</span><span class="marquee-item">◆</span>
        <span class="marquee-item">Jaipur HQ</span><span class="marquee-item">◆</span>
      </div>
    </div>
    <div class="marquee-container marquee-row-2">
      <div class="marquee-content" id="marquee2">
        <span class="marquee-item">10,000+ Retailers</span><span class="marquee-item">&rarr;</span>
        <span class="marquee-item">&#8377;50Cr+ Processed</span><span class="marquee-item">&rarr;</span>
        <span class="marquee-item">7 Services</span><span class="marquee-item">&rarr;</span>
        <span class="marquee-item">99.9% Uptime</span><span class="marquee-item">&rarr;</span>
        <span class="marquee-item">Jaipur HQ</span><span class="marquee-item">&rarr;</span>
      </div>
    </div>
  </section>

  <!-- ABOUT US SECTION -->
  <section class="about-section" id="about" style="padding: 2rem 0; background: var(--surface);">
    <div class="container" style="display: flex; flex-wrap: wrap; align-items: center; gap: 4rem;">
      <div style="flex: 1; min-width: 300px;">
        <div class="section-label">About Us</div>
        <h2 style="font-size: 2.5rem; margin-bottom: 1.5rem; color: var(--text-head); line-height: 1.2;">Your Trusted Partner in Digital Payments</h2>
        <p style="font-size: 1.1rem; color: var(--text-body); margin-bottom: 1.5rem; line-height: 1.6;">At <%=company.Name %>, we bridge the gap between traditional retail and modern fintech. By equipping shop owners with advanced yet accessible tools, we transform local stores into digital banking hubs.</p>
        <ul style="list-style: none; display: flex; flex-direction: column; gap: 1rem; margin-bottom: 2rem;">
          <li style="display: flex; align-items: center; gap: 1rem;"><span style="color: var(--gold); font-size: 1.2rem;">✓</span> <span style="font-weight: 500; color: var(--text-head);">B2B Model Built for Retailers</span></li>
          <li style="display: flex; align-items: center; gap: 1rem;"><span style="color: var(--gold); font-size: 1.2rem;">✓</span> <span style="font-weight: 500; color: var(--text-head);">Bank-Grade Security</span></li>
          <li style="display: flex; align-items: center; gap: 1rem;"><span style="color: var(--gold); font-size: 1.2rem;">✓</span> <span style="font-weight: 500; color: var(--text-head);">10,000+ Active Retail Partners</span></li>
        </ul>
      </div>
      <div style="flex: 1.3; min-width: 300px; display: flex; justify-content: center; align-items: center;">
        <div style="background: var(--bg-section); border-radius: 24px; padding: 0.5rem; border: 1px solid var(--border-color); box-shadow: 0 20px 40px rgba(0,0,0,0.05); width: 100%; text-align: center;">
           <img src="siteadmin/images/about1.png" alt="About <%=company.Name %>" style="width: 100%; height: auto; border-radius: 20px; display: block; filter: drop-shadow(0 10px 20px rgba(0,0,0,0.1));">
        </div>
      </div>
    </div>
  </section>

  <!-- SERVICES SECTION (7 Cards) -->
  <section class="services-section" id="services">
    <div class="container">
      <div class="section-header">
        <div class="section-label">What We Offer</div>
        <h2>Comprehensive Digital Suite</h2>
      </div>
      
      <div class="bento-grid">
        <!-- Card 1 Wide -->
        <div class="bento-wrap bento-wide">
          <div class="bento-card">
            <div class="srv-bg-num">01</div>
            <div class="srv-icon">📱</div>
            <h3>RECHARGE</h3>
            <p>Mobile & DTH Recharge with all major operators supported across India with real-time success rates.</p>
            <a href="#" class="explore-link">Explore Service &rarr;</a>
          </div>
        </div>
        
        <!-- Card 2 Normal -->
        <div class="bento-wrap">
          <div class="bento-card">
            <div class="srv-bg-num">02</div>
            <div class="srv-icon">⚡</div>
            <h3>BBPS</h3>
            <p>Bharat Bill Payment for Electricity, Water, Gas, FASTag with instant confirmation.</p>
            <a href="#" class="explore-link">Explore Service &rarr;</a>
          </div>
        </div>
        
        <!-- Card 3 Normal -->
        <div class="bento-wrap">
          <div class="bento-card">
            <div class="srv-bg-num">03</div>
            <div class="srv-icon">🏧</div>
            <h3>AEPS</h3>
            <p>Aadhaar Enabled Payment System. Convert your shop into an ATM securely.</p>
            <a href="#" class="explore-link">Explore Service &rarr;</a>
          </div>
        </div>
        
        <!-- Card 4 Normal -->
        <div class="bento-wrap">
          <div class="bento-card">
            <div class="srv-bg-num">04</div>
            <div class="srv-icon">💳</div>
            <h3>CCPAY</h3>
            <p>Facilitate easy and instant credit card bill payments for your customers.</p>
            <a href="#" class="explore-link">Explore Service &rarr;</a>
          </div>
        </div>
        
        <!-- Card 5 Normal -->
        <div class="bento-wrap">
          <div class="bento-card">
            <div class="srv-bg-num">05</div>
            <div class="srv-icon">🏦</div>
            <h3>UPI CASHOUT</h3>
            <p>Seamlessly offer UPI cash withdrawal services for your customers.</p>
            <a href="#" class="explore-link">Explore Service &rarr;</a>
          </div>
        </div>
        
        <!-- Card 6 Wide -->
        <div class="bento-wrap bento-wide">
          <div class="bento-card">
            <div class="srv-bg-num">06</div>
            <div class="srv-icon">👤</div>
            <h3>AADHAR PAY</h3>
            <p>Accept payments directly via Aadhaar numbers effortlessly and securely.</p>
            <a href="#" class="explore-link">Explore Service &rarr;</a>
          </div>
        </div>
        
        <!-- Card 7 Normal -->
        <div class="bento-wrap">
          <div class="bento-card">
            <div class="srv-bg-num">07</div>
            <div class="srv-icon">💸</div>
            <h3>DMT</h3>
            <p>Domestic Money Transfer to any bank account in India 24/7 safely.</p>
            <a href="#" class="explore-link">Explore Service &rarr;</a>
          </div>
        </div>
      </div>
    </div>
  </section>

  <!-- FEATURES SECTION -->
  <section id="features" class="features-section">
    <div class="container">
      <div class="section-label" style="text-align:center; display:block;">Platform Advantages</div>
      <h2 style="text-align:center; font-size:3rem; margin-bottom:4rem;" class="font-playfair">Why Retailers Choose Us</h2>
      
      <div class="features-grid">
        <div class="feature-block slide-in-left">
          <div class="f-num">01</div>
          <div class="f-content">
            <h3>Fast & Easy</h3>
            <p>Designed for speed. Perform transactions in seconds with our optimized interface.</p>
            <ul class="f-list">
              <li>Instant Processing</li>
              <li>One-tap Recharge</li>
              <li>Real-time Status</li>
            </ul>
          </div>
        </div>
        
        <div class="feature-block slide-in-right">
          <div class="f-num">02</div>
          <div class="f-content">
            <h3>Access Anywhere</h3>
            <p>Your business travels with you. Operate from any device, anytime.</p>
            <ul class="f-list">
              <li>Mobile App (Android)</li>
              <li>Desktop Portal</li>
              <li>Any Web Browser</li>
            </ul>
          </div>
        </div>
        
        <div class="feature-block slide-in-left">
          <div class="f-num">03</div>
          <div class="f-content">
            <h3>Secure Transactions</h3>
            <p>Bank-grade security protocols ensuring your money is always safe.</p>
            <ul class="f-list">
              <li>256-bit Encryption</li>
              <li>2FA Login</li>
              <li>Fraud Detection</li>
            </ul>
          </div>
        </div>
        
        <div class="feature-block slide-in-right">
          <div class="f-num">04</div>
          <div class="f-content">
            <h3>24/7 Support</h3>
            <p>We are always here to help you resolve issues and grow your business.</p>
            <ul class="f-list">
              <li>Live Chat</li>
              <li>Phone Support</li>
              <li>Ticket System</li>
            </ul>
          </div>
        </div>
      </div>
    </div>
  </section>

  <!-- WHY CHOOSE US (DARK) -->
  <section class="why-section">
    <div class="container why-grid">
      <div class="why-left">
        <div class="section-label" style="color: var(--gold-light);">The <%=company.Name %> Edge</div>
        <h2 class="font-playfair">Built For Retailers.<br>Trusted By Thousands.</h2>
        
        <div class="check-list">
          <div class="check-item">
            <div class="check-icon">✓</div>
            <div class="check-text"><span class="check-val count-up" data-target="7">7</span> High-Margin Services</div>
          </div>
          <div class="check-item">
            <div class="check-icon">✓</div>
            <div class="check-text"><span class="check-val count-up" data-target="10000">10,000</span>+ Active Retailers</div>
          </div>
          <div class="check-item">
            <div class="check-icon">✓</div>
            <div class="check-text">&#8377;<span class="check-val count-up" data-target="50">50</span>Cr+ Monthly Processed</div>
          </div>
          <div class="check-item">
            <div class="check-icon">✓</div>
            <div class="check-text"><span class="check-val count-up" data-target="99">99</span>.9% Server Uptime</div>
          </div>
        </div>
        
        <button class="btn-primary magnetic-btn" style="margin-top: 1rem;">Join the Network</button>
      </div>
      
      <div class="why-right">
        <div class="phone-glow"></div>
        <div class="phone-mockup">
          <div class="phone-screen">
            <div style="text-align:center; margin-bottom: 2rem;">
              <div style="font-size: 0.8rem; color: var(--text-body); text-transform:uppercase; margin-bottom:5px;">Total Earnings</div>
              <div class="font-mono" style="font-size: 2.2rem; color: var(--gold); font-weight:bold;">&#8377;4,250</div>
            </div>
            
            <div style="display:grid; grid-template-columns: 1fr 1fr; gap: 10px; margin-bottom: 2rem;">
              <div style="background: var(--surface); border: 1px solid var(--border-color); padding: 15px; border-radius: 12px; text-align:center;">
                <div style="font-size:1.5rem; margin-bottom:5px;">📱</div>
                <div style="font-size:0.7rem; color:var(--text-head); font-weight:600;">Recharge</div>
              </div>
              <div style="background: var(--surface); border: 1px solid var(--border-color); padding: 15px; border-radius: 12px; text-align:center;">
                <div style="font-size:1.5rem; margin-bottom:5px;">⚡</div>
                <div style="font-size:0.7rem; color:var(--text-head); font-weight:600;">BBPS</div>
              </div>
            </div>
            
            <div style="background: var(--surface); border: 1px solid var(--border-color); border-radius: 12px; padding: 15px; margin-bottom: 10px;">
              <div style="display:flex; justify-content:space-between; align-items:center; border-bottom: 1px solid var(--border-color); padding-bottom:10px;">
                <span style="font-size:0.8rem; color:var(--text-head); font-weight:600;">Today's Txn</span>
                <span style="font-size:0.8rem; color:var(--emerald-accent);">24 Successful</span>
              </div>
              <div style="padding-top:10px; display:flex; align-items:center; gap:10px;">
                <div style="width:30px;height:30px;background:var(--gold-glow);border-radius:50%;display:flex;align-items:center;justify-content:center;font-size:0.8rem;">🏧</div>
                <div style="flex-grow:1;">
                  <div style="font-size:0.8rem; color:var(--text-head); font-weight:600;">Cash Withdrawal</div>
                  <div style="font-size:0.65rem; color:var(--text-muted);">Aadhaar Pay</div>
                </div>
                <div style="font-size:0.8rem; color:var(--text-head); font-weight:bold; font-family:monospace;">&#8377;2,000</div>
              </div>
            </div>
          </div>
          
          <div class="notification notif-1">
            <div style="color:var(--emerald-accent); font-size:1.2rem;">✅</div>
            <div>&#8377;299 Jio Recharge Done</div>
          </div>
          <div class="notification notif-2">
            <div style="color:var(--gold); font-size:1.2rem;">💰</div>
            <div>Commission Credited</div>
          </div>
        </div>
      </div>
    </div>
  </section>

  <!-- TESTIMONIALS -->
  <section class="test-section">
    <div class="container" style="text-align:center; margin-bottom: 4rem;">
      <div class="section-label">Success Stories</div>
      <h2 class="font-playfair" style="font-size: 3rem;">Hear From Our Partners</h2>
    </div>
    
        <div class="test-marquee test-row-1">
      <div class="test-card-wrap" style="display:flex; gap:2rem;">

        <!-- Card 1 -->
        <div class="test-card tilt-card">
          <div class="quote-mark">"</div>
          <div class="stars">★★★★★</div>
          <p class="test-text">"<%=company.Name %> has transformed my small shop. The MATM service attracts so many customers, and my income has doubled in 3 months."</p>
          <div class="test-author">
            <div class="avatar" style="width:40px;height:40px;font-size:1rem;background:var(--deep);">R</div>
            <div>
              <div style="font-weight:700; font-family:'Sora';">Rashid Khan</div>
              <div class="author-badge">Retail Partner</div>
            </div>
          </div>
        </div>
        <!-- Card 2 -->
        <div class="test-card tilt-card">
          <div class="quote-mark">"</div>
          <div class="stars">★★★★★</div>
          <p class="test-text">"Best BBPS portal I have used. Zero downtime and instant settlement. Their customer support is always reachable."</p>
          <div class="test-author">
            <div class="avatar" style="width:40px;height:40px;font-size:1rem;background:var(--emerald-accent);">S</div>
            <div>
              <div style="font-weight:700; font-family:'Sora';">Salima</div>
              <div class="author-badge">Distributor</div>
            </div>
          </div>
        </div>
        <!-- Card 3 -->
        <div class="test-card tilt-card">
          <div class="quote-mark">"</div>
          <div class="stars">★★★★★</div>
          <p class="test-text">"Payouts are instant and secure. I trust them with large volumes of DMT transactions daily without a single failure."</p>
          <div class="test-author">
            <div class="avatar" style="width:40px;height:40px;font-size:1rem;background:var(--charcoal);">R</div>
            <div>
              <div style="font-weight:700; font-family:'Sora';">Ramesh Kumar</div>
              <div class="author-badge">Retail Partner</div>
            </div>
          </div>
        </div>
        <!-- Card 4 -->
        <div class="test-card tilt-card">
          <div class="quote-mark">"</div>
          <div class="stars">★★★★★</div>
          <p class="test-text">"The best platform for all my digital banking needs. Highly recommend it to all shop owners."</p>
          <div class="test-author">
            <div class="avatar" style="width:40px;height:40px;font-size:1rem;background:var(--gold);">A</div>
            <div>
              <div style="font-weight:700; font-family:'Sora';">Aamir Khan</div>
              <div class="author-badge">Distributor</div>
            </div>
          </div>
        </div>
      </div>
      <!-- Duplicate for infinite loop -->
      <div class="test-card-wrap" style="display:flex; gap:2rem;">

        <!-- Card 1 -->
        <div class="test-card tilt-card">
          <div class="quote-mark">"</div>
          <div class="stars">★★★★★</div>
          <p class="test-text">"<%=company.Name %> has transformed my small shop. The MATM service attracts so many customers, and my income has doubled in 3 months."</p>
          <div class="test-author">
            <div class="avatar" style="width:40px;height:40px;font-size:1rem;background:var(--deep);">R</div>
            <div>
              <div style="font-weight:700; font-family:'Sora';">Rashid Khan</div>
              <div class="author-badge">Retail Partner</div>
            </div>
          </div>
        </div>
        <!-- Card 2 -->
        <div class="test-card tilt-card">
          <div class="quote-mark">"</div>
          <div class="stars">★★★★★</div>
          <p class="test-text">"Best BBPS portal I have used. Zero downtime and instant settlement. Their customer support is always reachable."</p>
          <div class="test-author">
            <div class="avatar" style="width:40px;height:40px;font-size:1rem;background:var(--emerald-accent);">S</div>
            <div>
              <div style="font-weight:700; font-family:'Sora';">Salima</div>
              <div class="author-badge">Distributor</div>
            </div>
          </div>
        </div>
        <!-- Card 3 -->
        <div class="test-card tilt-card">
          <div class="quote-mark">"</div>
          <div class="stars">★★★★★</div>
          <p class="test-text">"Payouts are instant and secure. I trust them with large volumes of DMT transactions daily without a single failure."</p>
          <div class="test-author">
            <div class="avatar" style="width:40px;height:40px;font-size:1rem;background:var(--charcoal);">R</div>
            <div>
              <div style="font-weight:700; font-family:'Sora';">Ramesh Kumar</div>
              <div class="author-badge">Retail Partner</div>
            </div>
          </div>
        </div>
        <!-- Card 4 -->
        <div class="test-card tilt-card">
          <div class="quote-mark">"</div>
          <div class="stars">★★★★★</div>
          <p class="test-text">"The best platform for all my digital banking needs. Highly recommend it to all shop owners."</p>
          <div class="test-author">
            <div class="avatar" style="width:40px;height:40px;font-size:1rem;background:var(--gold);">A</div>
            <div>
              <div style="font-weight:700; font-family:'Sora';">Aamir Khan</div>
              <div class="author-badge">Distributor</div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </section>

  <!-- PROCESS / HOW IT WORKS -->
  <section id="process" class="process-section">
    <div class="container">
      <div class="section-label">Simple Onboarding</div>
      <h2 class="font-playfair" style="font-size: 3rem; margin-bottom: 2rem;">How It Works</h2>
      
      <div class="process-wrapper" id="processWrapper">
        <div class="process-line"></div>
        <div class="process-line-active" id="processLine"></div>
        
        <div class="process-step">
          <div class="step-circle">1</div>
          <h4>Register Free</h4>
          <p>Sign up with basic KYC documents instantly.</p>
        </div>
        <div class="process-step">
          <div class="step-circle">2</div>
          <h4>Add Wallet Balance</h4>
          <p>Load your wallet via UPI, Netbanking or Cards.</p>
        </div>
        <div class="process-step">
          <div class="step-circle">3</div>
          <h4>Choose Service</h4>
          <p>Select from BBPS, Recharge, MATM or DMT.</p>
        </div>
        <div class="process-step">
          <div class="step-circle">4</div>
          <h4>Start Earning</h4>
          <p>Earn high commissions on every successful transaction.</p>
        </div>
      </div>
    </div>
  </section>

  <!-- CTA FINALE -->
  <section class="cta-section">
    <div class="cta-glow"></div>
    <div class="noise-overlay"></div>
    <div class="container cta-content">
      <div class="section-label" style="color:var(--gold-light);">JOIN 10,000+ RETAILERS</div>
      <h2 class="font-playfair">Your Digital Finance<br>Business Starts Here.</h2>
      <p>Elevate your retail business with India's most trusted B2B fintech platform. No hidden fees, instant settlements.</p>
      
      <div class="cta-btns">
        <button class="btn-primary large magnetic-btn">Register Free &rarr;</button>
        <button class="btn-outline-white">📞 +91 93510 51856</button>
      </div>
      
      <div class="trust-badges">
        <div class="trust-badge"><span style="color:var(--gold);">🔐</span> Secure</div>
        <div class="trust-badge"><span style="color:var(--gold);">⚡</span> Instant</div>
        <div class="trust-badge"><span style="color:var(--gold);">🏆</span> Trusted</div>
      </div>
    </div>
  </section>
</asp:Content>

