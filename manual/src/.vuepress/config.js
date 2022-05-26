module.exports = {
    base: '/EnumRun/',
    title: 'EnumRun',
    port: '8080',
    locales: {
      '/': {
        lang: 'ja'
      },
    },
    themeConfig: {
      nav: [
          { text: "Top", link: "/" },
          { text: "GitHub", link: "https://github.com/tgiqfe/EnumRun" }
      ],
      sidebar: [
        {
          title: 'Guide',
          path: '/Guide/',
          collapsable: true,
          sidebarDepth: 2,
          children: [
              '/Guide/guide_01.md',
              '/Guide/guide_02.md',
              '/Guide/guide_03.md'
          ]
        },
        {
          title: 'EnumRun',
          path: '/EnumRun/',
          collapsable: true,
          sidebarDepth: 2,
          children: [
            '/EnumRun/execute_01.md',
            '/EnumRun/execute_02.md'
          ]
        },
        {
          title: 'ScriptDelivery',
          path: '/ScriptDelivery/',
          collapse: true,
          sidebarDepth: 2,
          children: [
            '/ScriptDelivery/scriptDelivery_01.md',
            '/ScriptDelivery/scriptDelivery_02.md'
          ]
        },
        {
          title: 'Setting',
          path: '/Setting/',
          collapsable: true,
          sidebarDepth: 2,
          children: [
              '/Setting/setting_01.md',
              '/Setting/setting_02.md',
              '/Setting/setting_03.md',
              '/Setting/setting_04.md',
              '/Setting/setting_05.md',
              '/Setting/setting_06.md',
              '/Setting/setting_07.md',
              '/Setting/setting_08.md',
              '/Setting/setting_09.md',
              '/Setting/setting_10.md'
          ]
        },
        {
            title: 'Sample',
            path: '/Sample/',
            collapsable: true,
            sidebarDepth: 2,
            children: [
                '/Sample/sample_01.md',
                '/Sample/sample_02.md'
            ]
          }
      ],
      sidebarDepth: 2,
      header:{
        background: '#555555'
      }
    },
    dest: '../docs/'
  }
  