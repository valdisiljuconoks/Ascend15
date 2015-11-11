var gulp = require('gulp');
var sass = require('gulp-sass');
var notify = require('gulp-notify');

var config = {
    bootstrapDir: './lib/bootstrap-sass',
    publicDir: './public'
};

gulp.task('sass', function() {
    return gulp.src('scss/**/*.scss')
        .pipe(sass({ includePaths: [config.bootstrapDir + '/assets/stylesheets'] }).on('error', sass.logError))
        .pipe(gulp.dest(config.publicDir))
        .pipe(notify('Css complete!'));
});

gulp.task('fonts', function() {
    return gulp.src(config.bootstrapDir + '/assets/fonts/**/*')
        .pipe(gulp.dest(config.publicDir + '/fonts'));
});

gulp.task('watch', function() {
    gulp.watch('scss/**/*.scss', ['sass']);
});

gulp.task('default', ['sass', 'fonts', 'watch']);