# DocSearch
文書検索システム  
  ・Elasticsearchを利用  
  ・共有フォルダをクロールし、WordやExcel、PDFのようなバイナリ形式の文書ファイルからテキストデータを抽出し、  
    ElasticsearchにIndexingする  
  ・機械学習により、入力したキーワードに関連する言葉をリストアップ。関連キーワードは、クロールした文書データから学習  
  ・検索された文書の類似文書を検索する機能も提供
 
 必要なライブラリ  
 NuGetでインストールできるもの  
  ・Elasticsearch.Net  
  ・NEST  
  ・SignalR Core、SignalR Client  
  ・Json.NET  
  ・Quartz.NET  
  ・Toxy  
 
 NuGetとは別に必要なもの  
  ・xdoc2txt  
  ・Elasticsearch  
  ・Kuromoji  
  ・Mecab  
  ・word2vec  
  ・jQueryFileTree  
  
