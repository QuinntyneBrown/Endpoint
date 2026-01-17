import type { Preview } from '@storybook/angular';

// Load Material Icons and Roboto font
const loadFonts = () => {
  const head = document.head;

  // Preconnect to Google Fonts
  const preconnect1 = document.createElement('link');
  preconnect1.rel = 'preconnect';
  preconnect1.href = 'https://fonts.googleapis.com';
  head.appendChild(preconnect1);

  const preconnect2 = document.createElement('link');
  preconnect2.rel = 'preconnect';
  preconnect2.href = 'https://fonts.gstatic.com';
  preconnect2.crossOrigin = 'anonymous';
  head.appendChild(preconnect2);

  // Load Roboto font
  const robotoLink = document.createElement('link');
  robotoLink.rel = 'stylesheet';
  robotoLink.href =
    'https://fonts.googleapis.com/css2?family=Roboto:wght@300;400;500&display=swap';
  head.appendChild(robotoLink);

  // Load Material Icons
  const iconsLink = document.createElement('link');
  iconsLink.rel = 'stylesheet';
  iconsLink.href = 'https://fonts.googleapis.com/icon?family=Material+Icons';
  head.appendChild(iconsLink);
};

loadFonts();

const preview: Preview = {
  parameters: {
    controls: {
      matchers: {
        color: /(background|color)$/i,
        date: /Date$/i,
      },
    },
    backgrounds: {
      default: 'dark',
      values: [
        {
          name: 'dark',
          value: '#0d1117',
        },
        {
          name: 'light',
          value: '#ffffff',
        },
      ],
    },
  },
};

export default preview;
