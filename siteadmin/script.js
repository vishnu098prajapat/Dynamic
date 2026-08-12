document.addEventListener('DOMContentLoaded', () => {
      
      // Theme Toggle
      const themeToggle = document.getElementById('themeToggle');
      const htmlElement = document.documentElement;
      
      themeToggle.addEventListener('click', () => {
        const currentTheme = htmlElement.getAttribute('data-theme');
        const newTheme = currentTheme === 'light' ? 'dark' : 'light';
        htmlElement.setAttribute('data-theme', newTheme);
        themeToggle.querySelector('.theme-thumb').textContent = newTheme === 'light' ? '🌙' : '☀️';
      });

      // Navbar Scroll Effect
      const navbar = document.getElementById('navbar');
      window.addEventListener('scroll', () => {
        if (window.scrollY > 80) {
          navbar.classList.add('scrolled');
        } else {
          navbar.classList.remove('scrolled');
        }
      });

      // Mobile Menu Toggle
      const mobileBtn = document.querySelector('.mobile-menu-btn');
      const navLinks = document.querySelector('.nav-links');
      if(mobileBtn && navLinks) {
        mobileBtn.addEventListener('click', () => {
          navLinks.classList.toggle('active');
        });
      }

      // Close mobile menu on link click
      document.querySelectorAll('.nav-links a').forEach(link => {
        link.addEventListener('click', () => {
          const menu = document.querySelector('.nav-links');
          if(menu) menu.classList.remove('active');
        });
      });

      // Hero H1 Word Animation
      const heroTitle = document.getElementById('heroTitle');
      if (heroTitle) {
        const text = heroTitle.textContent.trim();
        heroTitle.innerHTML = '';
        text.split(' ').forEach((word, index) => {
          if(word === '') return;
          const wrapper = document.createElement('span');
          wrapper.className = 'word-wrap';
          if(index > 0) wrapper.style.marginLeft = '0.3em'; // space
          
          const inner = document.createElement('span');
          inner.className = 'word';
          inner.textContent = word;
          inner.style.animationDelay = `${index * 0.08}s`;
          
          wrapper.appendChild(inner);
          heroTitle.appendChild(wrapper);
        });
      }

      // Magnetic Buttons Effect
      const magneticBtns = document.querySelectorAll('.magnetic-btn');
      magneticBtns.forEach(btn => {
        btn.addEventListener('mousemove', (e) => {
          const rect = btn.getBoundingClientRect();
          const x = e.clientX - rect.left - rect.width / 2;
          const y = e.clientY - rect.top - rect.height / 2;
          // max 8px offset
          const offsetX = (x / rect.width) * 16; 
          const offsetY = (y / rect.height) * 16;
          btn.style.transform = `translate(${offsetX}px, ${offsetY}px)`;
        });
        btn.addEventListener('mouseleave', () => {
          btn.style.transform = `translate(0px, 0px)`;
        });
      });

      // 3D Parallax on Hero Card Stack
      const heroVisual = document.getElementById('heroVisual');
      const cardStack = document.querySelector('.card-stack');
      if (heroVisual && cardStack) {
        document.querySelector('.hero').addEventListener('mousemove', (e) => {
          const x = (window.innerWidth / 2 - e.pageX) / 50;
          const y = (window.innerHeight / 2 - e.pageY) / 50;
          cardStack.style.transform = `rotateY(${x}deg) rotateX(${y}deg)`;
        });
        document.querySelector('.hero').addEventListener('mouseleave', () => {
          cardStack.style.transform = `rotateY(0deg) rotateX(0deg)`;
        });
      }

      // 3D Bento Card Hover
      const bentoWraps = document.querySelectorAll('.bento-wrap');
      bentoWraps.forEach(wrap => {
        const card = wrap.querySelector('.bento-card');
        wrap.addEventListener('mousemove', (e) => {
          const rect = wrap.getBoundingClientRect();
          const x = e.clientX - rect.left - rect.width / 2;
          const y = e.clientY - rect.top - rect.height / 2;
          // rotateX/Y based on mouse position
          const rotateY = (x / rect.width) * -4; // max 2deg
          const rotateX = (y / rect.height) * 6; // max 3deg
          card.style.transform = `translateY(-10px) rotateX(${rotateX}deg) rotateY(${rotateY}deg)`;
        });
        wrap.addEventListener('mouseleave', () => {
          card.style.transform = ``;
        });
      });

      // 3D Testimonial Card Hover
      const testCards = document.querySelectorAll('.tilt-card');
      testCards.forEach(card => {
        card.addEventListener('mousemove', (e) => {
          const rect = card.getBoundingClientRect();
          const x = e.clientX - rect.left - rect.width / 2;
          const y = e.clientY - rect.top - rect.height / 2;
          const rotateY = (x / rect.width) * 10; // max 5deg
          const rotateX = (y / rect.height) * -10; // max 5deg
          card.style.transform = `rotateX(${rotateX}deg) rotateY(${rotateY}deg)`;
        });
        card.addEventListener('mouseleave', () => {
          card.style.transform = ``;
        });
      });

      // Duplicate marquee content for seamless loop
      const marquee1 = document.getElementById('marquee1');
      if(marquee1) marquee1.innerHTML += marquee1.innerHTML;
      const marquee2 = document.getElementById('marquee2');
      if(marquee2) marquee2.innerHTML += marquee2.innerHTML;

      // Intersection Observers for Scroll Animations
      const observerOptions = { threshold: 0.15, rootMargin: '0px 0px -50px 0px' };
      
      // Numbers Count Up
      const countUpObserver = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
          if(entry.isIntersecting) {
            const el = entry.target;
            const target = parseInt(el.getAttribute('data-target'));
            let count = 0;
            const duration = 2000;
            const stepTime = Math.abs(Math.floor(duration / target));
            
            const timer = setInterval(() => {
              count += Math.ceil(target / 50) || 1;
              if (count >= target) {
                el.innerText = target;
                clearInterval(timer);
              } else {
                el.innerText = count;
              }
            }, stepTime);
            
            countUpObserver.unobserve(el);
          }
        });
      }, observerOptions);
      document.querySelectorAll('.count-up').forEach(el => countUpObserver.observe(el));

      // Process Line Animation
      const processObserver = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
          if (entry.isIntersecting) {
            const line = document.getElementById('processLine');
            const steps = document.querySelectorAll('.process-step');
            
            line.style.width = '100%';
            
            steps.forEach((step, index) => {
              setTimeout(() => {
                step.classList.add('active');
              }, index * 400 + 200);
            });
            
            processObserver.unobserve(entry.target);
          }
        });
      }, { threshold: 0.3 });
      const processWrap = document.getElementById('processWrapper');
      if(processWrap) processObserver.observe(processWrap);
      
      // Feature Slide-in
      const slideObserver = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
          if (entry.isIntersecting) {
            entry.target.style.opacity = '1';
            entry.target.style.transform = 'translateX(0)';
            slideObserver.unobserve(entry.target);
          }
        });
      }, observerOptions);
      
      document.querySelectorAll('.feature-block').forEach((el, index) => {
        el.style.opacity = '0';
        el.style.transform = index % 2 === 0 ? 'translateX(-50px)' : 'translateX(50px)';
        el.style.transition = 'all 0.6s ease-out';
        slideObserver.observe(el);
      });

      // Floating Particles Animation
      const createParticles = () => {
        const hero = document.querySelector('.hero-bg-elements');
        for(let i=0; i<20; i++) {
          const dot = document.createElement('div');
          dot.style.position = 'absolute';
          dot.style.width = Math.random() * 4 + 2 + 'px';
          dot.style.height = dot.style.width;
          dot.style.background = 'var(--gold)';
          dot.style.borderRadius = '50%';
          dot.style.opacity = Math.random() * 0.5 + 0.1;
          
          let x = Math.random() * window.innerWidth;
          let y = Math.random() * window.innerHeight;
          let speedX = (Math.random() - 0.5) * 1;
          let speedY = (Math.random() - 0.5) * 1;
          
          dot.style.left = x + 'px';
          dot.style.top = y + 'px';
          hero.appendChild(dot);
          
          const move = () => {
            x += speedX; y += speedY;
            if(x > window.innerWidth || x < 0) speedX *= -1;
            if(y > window.innerHeight || y < 0) speedY *= -1;
            dot.style.left = x + 'px';
            dot.style.top = y + 'px';
            requestAnimationFrame(move);
          };
          requestAnimationFrame(move);
        }
      };
      createParticles();
      
    });
