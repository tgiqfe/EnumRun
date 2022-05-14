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
          title: '準備',
          path: '/Prepare/',
          collapsable: true,
          sidebarDepth: 2,
          children: [
              '/Prepare/prepare_01.md',
              '/Prepare/prepare_02.md',
              '/Prepare/prepare_03.md'
          ]
        },
        {
          title: '実行',
          path: '/Execute/',
          collapsable: true,
          sidebarDepth: 2,
          children: [
            '/Execute/execute_01.md'
            '/Execute/execute_02.md'
          ]
        },
        {
          title: '設定',
          path: '/Setting/',
          collapsable: true,
          sidebarDepth: 2,
          children: [
              '/Setting/setting_01.md',
              '/Setting/setting_02.md',
              '/Setting/setting_03.md',
              '/Setting/setting_04.md',
              '/Setting/setting_05.md',
              '/Setting/setting_06.md'
          ]
        },
        {
            title: '補足',
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
    },
    dest: '../docs/'
  }
  