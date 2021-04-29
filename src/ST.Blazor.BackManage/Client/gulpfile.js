var gulp = require('gulp'),
  cleanCss = require('gulp-clean-css'),
  less = require('gulp-less'),
  rename = require('gulp-rename'),
  concatCss = require("gulp-concat-css"),
  npmImport = require("less-plugin-npm-import");

gulp.task('less', function () {
  return gulp
    .src([
      '**/*.less',
      '!node_modules/**',
      '!**/bin/**',
      '!**/obj/**'
    ])
    .pipe(less({
      javascriptEnabled: true,
      plugins: [new npmImport({ prefix: '~' })]
    }))
    .pipe(concatCss('site.css'))
    .pipe(cleanCss({ compatibility: '*' }))
    .pipe(gulp.dest('wwwroot/css'));
});

gulp.task('default', gulp.parallel('less'), function () { })