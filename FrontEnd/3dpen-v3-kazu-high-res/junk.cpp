


#define PICTURE_WIDTH 2

#define SCREEN_CAPTURE 1
#define DOT_SIZE 0.06





void Stroke::Draw(void)
{
	float xx, yy;

	int ii;

	glNormal3f(0.0, 0.0, 1.0);
	glLineWidth(PICTURE_WIDTH);

	//if(show_ink || pstate.TipDown) {
	if(show_ink || 
		//(pstate.ShowPen && this == edit_stroke) ||
		//(pstate.ShowPen  && this == stroke_data && (DoingEdit || !StartNewStroke || pstate.PenTilt < 0.0) )) {
		(pstate.ShowPen && this == edit_stroke) ||
		(pstate.ShowPen && this == stroke_data && !StartNewStroke ) ||
		(pstate.ShowPen  && (DoingEdit || ((pstate.Button1 || pstate.Button2) && StartNewStroke) ) )  ) {
		//(pstate.ShowPen  && this == stroke_data && (DoingEdit || !StartNewStroke || pstate.Button1 || pstate.Button2) )) {
		// draw the ink 
		
#ifdef SCREEN_CAPTURE
		glColor3f(0.2,0.2,0.2);
		glColor3f(0.0, 0.0, 0.0);
#else 
		glColor3f(0.0,1.0,0.0);
#endif






... there is more to this function

	glLineWidth(1.0);

	}



void Segment::Draw(void)
{

	double a, xx, yy;
	int ii, jj;

	//cout << "length is :" << FitLength() << endl;
	glNormal3f(0.0, 0.0, 1.0);

	glLineWidth(PICTURE_WIDTH);

	if(type == LINE_SEG) {

		glBegin(GL_LINE_STRIP);

		xx = (float) line.sx/INK_SCALE;
		yy = (float) line.sy/INK_SCALE;
		glVertex3f(xx, yy, (float) 0.07);

		xx = (float) line.ex/INK_SCALE;
		yy = (float) line.ey/INK_SCALE;
		glVertex3f(xx, yy, (float) 0.07);

		glEnd();

#ifdef SCREEN_CAPTURE
		class GLUquadric *quad;
		quad = gluNewQuadric();
		gluQuadricNormals(quad,GLU_SMOOTH);
		
		glPushMatrix();
		xx = (float) line.sx/INK_SCALE;
		yy = (float) line.sy/INK_SCALE;
		glTranslatef(xx, yy, 0.07);
		gluDisk(quad, 0.0, DOT_SIZE, 20, 20);
		glPopMatrix();

		glPushMatrix();
		xx = (float) line.ex/INK_SCALE;
		yy = (float) line.ey/INK_SCALE;
		glTranslatef(xx, yy, 0.07);
		gluDisk(quad, 0.0, DOT_SIZE, 20, 20);
		glPopMatrix();
		gluDeleteQuadric(quad);


#endif

	} else {
		glBegin(GL_LINE_STRIP);

		for(jj=0;jj<=25;jj++) {
			a = (arc.eang -arc.sang) * (double) jj / 25.0 + arc.sang;
			xx = (float)(arc.cx + arc.rad*cos(a)) /INK_SCALE;
			yy = (float)(arc.cy + arc.rad*sin(a)) /INK_SCALE;
			glVertex3f(xx, yy, (float) 0.07);

		}
		glEnd();


#ifdef SCREEN_CAPTURE
		class GLUquadric *quad2;
		quad2 = gluNewQuadric();
		gluQuadricNormals(quad2,GLU_SMOOTH);
		
		glPushMatrix();
		a = arc.sang;
		xx = (float)(arc.cx + arc.rad*cos(a)) /INK_SCALE;
		yy = (float)(arc.cy + arc.rad*sin(a)) /INK_SCALE;
		glTranslatef(xx, yy, 0.07);
		gluDisk(quad2, 0.0, DOT_SIZE, 20, 20);
		glPopMatrix();

		glPushMatrix();
		a = arc.eang;
		xx = (float)(arc.cx + arc.rad*cos(a)) /INK_SCALE;
		yy = (float)(arc.cy + arc.rad*sin(a)) /INK_SCALE;
		glTranslatef(xx, yy, 0.07);
		gluDisk(quad2, 0.0, DOT_SIZE, 20, 20);
		glPopMatrix();
		gluDeleteQuadric(quad2);


#endif




	}

	glLineWidth(1.0);
}






void SegmentList::Draw(void)
{
	int ii;
	glNormal3f(0.0,0.0,1.0);
	for(ii=0; ii<GetSize(); ii++) {
		if(ii % 2) {
			glColor3f(0.0, 0.0, 1.0);
		} else {
			glColor3f(1.0, 0.0, 0.0);
		}
#ifdef SCREEN_CAPTURE
		glColor3f(0.0, 0.0, 0.0);
#endif
		GetAt(ii)->Draw();
	}
}
